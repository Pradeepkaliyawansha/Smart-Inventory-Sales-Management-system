import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, startWith, map } from 'rxjs';
import { SaleService } from '../../../core/services/sale.service';
import { ProductService } from '../../../core/services/product.service';
import { CustomerService } from '../../../core/services/customer.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Product } from '../../../core/models/product.model';
import { Customer } from '../../../core/models/customer.model';
import { PaymentMethod } from '../../../core/models/sale.model';

@Component({
  selector: 'app-sale-form',
  templateUrl: './sale-form.component.html',
  styleUrls: ['./sale-form.component.scss'],
})
export class SaleFormComponent implements OnInit {
  saleForm!: FormGroup;
  loading = false;
  products: Product[] = [];
  customers: Customer[] = [];
  filteredCustomers!: Observable<Customer[]>;
  paymentMethods = Object.values(PaymentMethod).filter(
    (value) => typeof value === 'number'
  );

  constructor(
    private formBuilder: FormBuilder,
    private saleService: SaleService,
    private productService: ProductService,
    private customerService: CustomerService,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.createForm();
    this.loadData();
    this.setupCustomerFilter();
  }

  private createForm(): void {
    this.saleForm = this.formBuilder.group({
      customerId: ['', Validators.required],
      customerSearch: [''],
      paymentMethod: [PaymentMethod.Cash, Validators.required],
      notes: [''],
      saleItems: this.formBuilder.array([]),
    });
  }

  private async loadData(): Promise<void> {
    try {
      this.loading = true;
      const [products, customers] = await Promise.all([
        this.productService.getProducts().toPromise(),
        this.customerService.getCustomers().toPromise(),
      ]);

      this.products = products!.filter(
        (p) => p.isActive && p.stockQuantity > 0
      );
      this.customers = customers!.filter((c) => c.isActive);

      // Add first sale item
      this.addSaleItem();
    } catch (error) {
      this.notificationService.showError('Error loading data');
    } finally {
      this.loading = false;
    }
  }

  private setupCustomerFilter(): void {
    this.filteredCustomers = this.saleForm
      .get('customerSearch')!
      .valueChanges.pipe(
        startWith(''),
        map((value) => this._filterCustomers(value || ''))
      );
  }

  private _filterCustomers(value: string): Customer[] {
    const filterValue = value.toLowerCase();
    return this.customers.filter(
      (customer) =>
        customer.name.toLowerCase().includes(filterValue) ||
        customer.email?.toLowerCase().includes(filterValue)
    );
  }

  get saleItems(): FormArray {
    return this.saleForm.get('saleItems') as FormArray;
  }

  addSaleItem(): void {
    const saleItem = this.formBuilder.group({
      productId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
      discountPercentage: [0, [Validators.min(0), Validators.max(100)]],
    });

    this.saleItems.push(saleItem);
  }

  removeSaleItem(index: number): void {
    if (this.saleItems.length > 1) {
      this.saleItems.removeAt(index);
    }
  }

  onProductChange(index: number): void {
    const saleItem = this.saleItems.at(index);
    const productId = saleItem.get('productId')?.value;
    const product = this.products.find((p) => p.id === +productId);

    if (product) {
      saleItem.patchValue({
        unitPrice: product.price,
      });
    }
  }

  onCustomerSelect(customer: Customer): void {
    this.saleForm.patchValue({
      customerId: customer.id,
      customerSearch: customer.name,
    });
  }

  getSubTotal(): number {
    return this.saleItems.controls.reduce((total, item) => {
      const quantity = item.get('quantity')?.value || 0;
      const unitPrice = item.get('unitPrice')?.value || 0;
      const discountPercentage = item.get('discountPercentage')?.value || 0;
      const itemTotal = quantity * unitPrice;
      const discount = itemTotal * (discountPercentage / 100);
      return total + (itemTotal - discount);
    }, 0);
  }

  getTaxAmount(): number {
    return this.getSubTotal() * 0.1; // 10% tax
  }

  getTotalAmount(): number {
    return this.getSubTotal() + this.getTaxAmount();
  }

  onSubmit(): void {
    if (this.saleForm.valid && this.saleItems.length > 0) {
      this.loading = true;

      const formData = {
        customerId: this.saleForm.get('customerId')?.value,
        paymentMethod: this.saleForm.get('paymentMethod')?.value,
        notes: this.saleForm.get('notes')?.value,
        saleItems: this.saleItems.value.map((item: any) => ({
          productId: +item.productId,
          quantity: item.quantity,
          unitPrice: item.unitPrice,
          discountPercentage: item.discountPercentage || 0,
        })),
      };

      this.saleService.createSale(formData).subscribe({
        next: (sale) => {
          this.notificationService.showSuccess('Sale created successfully');
          this.router.navigate(['/sales', sale.id, 'invoice']);
        },
        error: () => {
          this.notificationService.showError('Error creating sale');
          this.loading = false;
        },
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.saleForm.controls).forEach((field) => {
      const control = this.saleForm.get(field);
      control?.markAsTouched({ onlySelf: true });
    });

    this.saleItems.controls.forEach((item) => {
      Object.keys(item.controls).forEach((field) => {
        const control = item.get(field);
        control?.markAsTouched({ onlySelf: true });
      });
    });
  }

  onCancel(): void {
    this.router.navigate(['/sales']);
  }

  getPaymentMethodName(method: PaymentMethod): string {
    const names: { [key: number]: string } = {
      [PaymentMethod.Cash]: 'Cash',
      [PaymentMethod.Card]: 'Card',
      [PaymentMethod.BankTransfer]: 'Bank Transfer',
      [PaymentMethod.Check]: 'Check',
      [PaymentMethod.Credit]: 'Credit',
    };
    return names[method] || 'Unknown';
  }
}
