import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  FormArray,
  Validators,
  AbstractControl,
  ReactiveFormsModule,
} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import { map, startWith } from 'rxjs/operators';
import { PaymentMethod } from '../../../core/models/sale.model';
import { Customer } from '../../../core/models/customer.model';
import { Product } from '../../../core/models/product.model';
import { SaleService } from '../../../core/services/sale.service';
import { CustomerService } from '../../../core/services/customer.service';
import { ProductService } from '../../../core/services/product.service';
import { NotificationService } from '../../../core/services/notification.service';

// Angular Material
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';

import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { SharedModule } from 'src/app/shared/shared.module';

@Component({
  selector: 'app-sale-form',
  standalone: true,
  imports: [
    SharedModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatDialogModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatChipsModule,
    MatAutocompleteModule,
  ],
  templateUrl: './sale-form.component.html',
  styleUrls: ['./sale-form.component.scss'],
})
export class SaleFormComponent implements OnInit {
  saleForm!: FormGroup;
  loading = false;
  customers: Customer[] = [];
  products: Product[] = [];
  filteredCustomers!: Observable<Customer[]>;
  paymentMethods = Object.values(PaymentMethod).filter(
    (value) => typeof value === 'number'
  );

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private saleService: SaleService,
    private customerService: CustomerService,
    private productService: ProductService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.createForm();
    this.loadData();
    this.setupCustomerFilter();
  }

  onCustomerSelect(customer: Customer): void {
    this.saleForm.patchValue({
      customerId: customer.id,
    });
  }

  // Fix the setupCustomerFilter method
  private setupCustomerFilter(): void {
    const customerSearchControl = this.saleForm.get('customerSearch');
    if (customerSearchControl) {
      this.filteredCustomers = customerSearchControl.valueChanges.pipe(
        startWith(''),
        map((value) => this._filterCustomers(value || ''))
      );
    }
  }

  // Update the form creation to include customerSearch
  private createForm(): void {
    this.saleForm = this.formBuilder.group({
      customerId: ['', Validators.required],
      customerSearch: [''], // Add this field for autocomplete
      paymentMethod: [PaymentMethod.Cash, Validators.required],
      notes: [''],
      saleItems: this.formBuilder.array([this.createSaleItem()]),
      subTotal: [{ value: 0, disabled: true }],
      discountAmount: [0],
      taxAmount: [{ value: 0, disabled: true }],
      totalAmount: [{ value: 0, disabled: true }],
    });

    // Subscribe to changes for calculations
    this.saleItems.valueChanges.subscribe(() => {
      this.calculateTotals();
    });
  }

  get saleItemsFormGroups(): FormGroup[] {
    return this.saleItems.controls as FormGroup[];
  }
  private createSaleItem(): FormGroup {
    return this.formBuilder.group({
      productId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
      discountPercentage: [0, [Validators.min(0), Validators.max(100)]],
      totalPrice: [{ value: 0, disabled: true }],
    });
  }

  get saleItems(): FormArray {
    return this.saleForm.get('saleItems') as FormArray;
  }

  addSaleItem(): void {
    this.saleItems.push(this.createSaleItem());
  }

  removeSaleItem(index: number): void {
    this.saleItems.removeAt(index);
  }

  onProductChange(index: number): void {
    const item = this.saleItems.at(index);
    const productId = item.get('productId')?.value;

    if (productId) {
      const product = this.products.find((p) => p.id === productId);
      if (product) {
        item.patchValue({
          unitPrice: product.price,
        });
        // Trigger calculation after price update
        this.onQuantityOrPriceChange(item);
      }
    }
  }

  // Also add this method to handle quantity/price changes
  onQuantityOrPriceChange(item: AbstractControl): void {
    const itemGroup = item as FormGroup;
    const quantity = itemGroup.get('quantity')?.value || 0;
    const unitPrice = itemGroup.get('unitPrice')?.value || 0;
    const discountPercentage = itemGroup.get('discountPercentage')?.value || 0;

    const subtotal = quantity * unitPrice;
    const discount = (subtotal * discountPercentage) / 100;
    const totalPrice = subtotal - discount;

    itemGroup.patchValue(
      {
        totalPrice: totalPrice,
      },
      { emitEvent: false }
    ); // Prevent infinite loop

    // Trigger total calculation
    this.calculateTotals();
  }

  getSubTotal(): number {
    let subTotal = 0;
    this.saleItems.controls.forEach((item) => {
      const quantity = item.get('quantity')?.value || 0;
      const unitPrice = item.get('unitPrice')?.value || 0;
      const discountPercentage = item.get('discountPercentage')?.value || 0;

      const itemSubtotal = quantity * unitPrice;
      const itemDiscount = (itemSubtotal * discountPercentage) / 100;
      subTotal += itemSubtotal - itemDiscount;
    });
    return subTotal;
  }

  getTaxAmount(): number {
    const subTotal = this.getSubTotal();
    const discountAmount = this.saleForm.get('discountAmount')?.value || 0;
    return (subTotal - discountAmount) * 0.1; // 10% tax
  }

  getTotalAmount(): number {
    const subTotal = this.getSubTotal();
    const discountAmount = this.saleForm.get('discountAmount')?.value || 0;
    const taxAmount = this.getTaxAmount();
    return subTotal - discountAmount + taxAmount;
  }

  private calculateTotals(): void {
    const subTotal = this.getSubTotal();
    const discountAmount = this.saleForm.get('discountAmount')?.value || 0;
    const taxAmount = this.getTaxAmount();
    const totalAmount = this.getTotalAmount();

    this.saleForm.patchValue(
      {
        subTotal: subTotal,
        taxAmount: taxAmount,
        totalAmount: totalAmount,
      },
      { emitEvent: false }
    );
  }

  private loadData(): void {
    // Load customers
    this.customerService.getCustomers().subscribe({
      next: (customers) => {
        this.customers = customers;
      },
      error: (error) => {
        this.notificationService.showError('Error loading customers');
        console.error('Error loading customers:', error);
      },
    });

    // Load products
    this.productService.getProducts().subscribe({
      next: (products) => {
        this.products = products;
      },
      error: (error) => {
        this.notificationService.showError('Error loading products');
        console.error('Error loading products:', error);
      },
    });
  }

  private _filterCustomers(value: string): Customer[] {
    const filterValue = value.toString().toLowerCase();
    return this.customers.filter((customer) =>
      customer.name.toLowerCase().includes(filterValue)
    );
  }

  getPaymentMethodName(method: PaymentMethod | string): string {
    const paymentMethod =
      typeof method === 'string' ? parseInt(method) : method;
    switch (paymentMethod) {
      case PaymentMethod.Cash:
        return 'Cash';
      case PaymentMethod.Card:
        return 'Card';
      case PaymentMethod.BankTransfer:
        return 'Bank Transfer';
      case PaymentMethod.Check:
        return 'Check';
      case PaymentMethod.Credit:
        return 'Credit';
      default:
        return 'Unknown';
    }
  }

  onSubmit(): void {
    if (this.saleForm.valid) {
      this.loading = true;

      // Simulate API call
      setTimeout(() => {
        console.log('Sale saved:', this.saleForm.getRawValue());
        this.router.navigate(['/sales']);
      }, 1000);
    } else {
      this.markFormGroupTouched();
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.saleForm.controls).forEach((field) => {
      const control = this.saleForm.get(field);
      control?.markAsTouched({ onlySelf: true });

      if (control instanceof FormArray) {
        control.controls.forEach((item: AbstractControl) => {
          if (item instanceof FormGroup) {
            Object.keys(item.controls).forEach((itemField) => {
              item.get(itemField)?.markAsTouched({ onlySelf: true });
            });
          }
        });
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/sales']);
  }
}
