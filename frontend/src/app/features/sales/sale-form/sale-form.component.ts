import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  FormArray,
  Validators,
  AbstractControl,
} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import { map, startWith } from 'rxjs/operators';
import { PaymentMethod } from '../../../core/models/sale.model';
import { Customer } from '../../../core/models/customer.model';
import { Product } from '../../../core/models/product.model';

@Component({
  selector: 'app-sale-form',
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
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.createForm();
    this.loadData();
    this.setupCustomerFilter();
  }

  private createForm(): void {
    this.saleForm = this.formBuilder.group({
      customerId: ['', Validators.required],
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

  onProductChange(item: AbstractControl, productId: number): void {
    const product = this.products.find((p) => p.id === productId);
    if (product) {
      const itemGroup = item as FormGroup;
      itemGroup.patchValue({
        unitPrice: product.price,
      });
    }
  }

  onQuantityOrPriceChange(item: AbstractControl): void {
    const itemGroup = item as FormGroup;
    const quantity = itemGroup.get('quantity')?.value || 0;
    const unitPrice = itemGroup.get('unitPrice')?.value || 0;
    const discountPercentage = itemGroup.get('discountPercentage')?.value || 0;

    const subtotal = quantity * unitPrice;
    const discount = (subtotal * discountPercentage) / 100;
    const totalPrice = subtotal - discount;

    itemGroup.patchValue({
      totalPrice: totalPrice,
    });
  }

  private calculateTotals(): void {
    let subTotal = 0;

    this.saleItems.controls.forEach((item) => {
      const totalPrice = item.get('totalPrice')?.value || 0;
      subTotal += totalPrice;
    });

    const discountAmount = this.saleForm.get('discountAmount')?.value || 0;
    const taxRate = 0.1; // 10% tax
    const taxAmount = (subTotal - discountAmount) * taxRate;
    const totalAmount = subTotal - discountAmount + taxAmount;

    this.saleForm.patchValue({
      subTotal: subTotal,
      taxAmount: taxAmount,
      totalAmount: totalAmount,
    });
  }

  private loadData(): void {
    // Mock data - replace with actual service calls
    this.customers = [
      {
        id: 1,
        name: 'John Doe',
        email: 'john@example.com',
        phone: '123-456-7890',
        address: '123 Main St',
        loyaltyPoints: 100,
        creditBalance: 0,
        isActive: true,
        createdAt: new Date(),
      },
    ];

    this.products = [
      {
        id: 1,
        name: 'Product A',
        price: 10.99,
        stockQuantity: 100,
        sku: 'PA001',
        barcode: '123456789',
        costPrice: 5.99,
        minStockLevel: 10,
        categoryId: 1,
        categoryName: 'Category A',
        supplierId: 1,
        supplierName: 'Supplier A',
        isActive: true,
        isLowStock: false,
        createdAt: new Date(),
        updatedAt: new Date(),
      },
    ];
  }

  private setupCustomerFilter(): void {
    this.filteredCustomers = this.saleForm.get('customerId')!.valueChanges.pipe(
      startWith(''),
      map((value) => this._filterCustomers(value))
    );
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
