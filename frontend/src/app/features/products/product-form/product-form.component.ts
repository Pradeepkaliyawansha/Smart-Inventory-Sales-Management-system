import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { NotificationService } from '../../../core/services/notification.service';
import {
  Product,
  Category,
  Supplier,
} from '../../../core/models/product.model';

@Component({
  selector: 'app-product-form',
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.scss'],
})
export class ProductFormComponent implements OnInit {
  productForm!: FormGroup;
  loading = false;
  isEditMode = false;
  productId: number | null = null;
  categories: Category[] = [];
  suppliers: Supplier[] = [];

  constructor(
    private formBuilder: FormBuilder,
    private productService: ProductService,
    private notificationService: NotificationService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.createForm();
    this.loadFormData();
    this.checkEditMode();
  }

  private createForm(): void {
    this.productForm = this.formBuilder.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', Validators.maxLength(500)],
      sku: ['', [Validators.required, Validators.maxLength(50)]],
      barcode: ['', [Validators.required, Validators.maxLength(50)]],
      price: [0, [Validators.required, Validators.min(0)]],
      costPrice: [0, [Validators.required, Validators.min(0)]],
      stockQuantity: [0, [Validators.required, Validators.min(0)]],
      minStockLevel: [0, [Validators.required, Validators.min(0)]],
      categoryId: ['', Validators.required],
      supplierId: ['', Validators.required],
      imageUrl: [''],
    });
  }

  private async loadFormData(): Promise<void> {
    try {
      // Load categories and suppliers
      // In a real app, these would come from separate API endpoints
      this.categories = [
        { id: 1, name: 'Electronics', isActive: true, createdAt: new Date() },
        { id: 2, name: 'Clothing', isActive: true, createdAt: new Date() },
        { id: 3, name: 'Books', isActive: true, createdAt: new Date() },
        { id: 4, name: 'Home & Garden', isActive: true, createdAt: new Date() },
      ];

      this.suppliers = [
        { id: 1, name: 'Supplier A', isActive: true, createdAt: new Date() },
        { id: 2, name: 'Supplier B', isActive: true, createdAt: new Date() },
        { id: 3, name: 'Supplier C', isActive: true, createdAt: new Date() },
      ];
    } catch (error) {
      this.notificationService.showError('Error loading form data');
    }
  }

  private checkEditMode(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.productId = +id;
      this.loadProduct(this.productId);
    }
  }

  private loadProduct(id: number): void {
    this.loading = true;
    this.productService.getProduct(id).subscribe({
      next: (product) => {
        this.productForm.patchValue({
          name: product.name,
          description: product.description,
          sku: product.sku,
          barcode: product.barcode,
          price: product.price,
          costPrice: product.costPrice,
          stockQuantity: product.stockQuantity,
          minStockLevel: product.minStockLevel,
          categoryId: product.categoryId,
          supplierId: product.supplierId,
          imageUrl: product.imageUrl,
        });
        this.loading = false;
      },
      error: () => {
        this.notificationService.showError('Error loading product');
        this.router.navigate(['/products']);
      },
    });
  }

  onSubmit(): void {
    if (this.productForm.valid) {
      this.loading = true;
      const formData = this.productForm.value;

      const request$ = this.isEditMode
        ? this.productService.updateProduct(this.productId!, formData)
        : this.productService.createProduct(formData);

      request$.subscribe({
        next: () => {
          const message = this.isEditMode
            ? 'Product updated successfully'
            : 'Product created successfully';
          this.notificationService.showSuccess(message);
          this.router.navigate(['/products']);
        },
        error: () => {
          const message = this.isEditMode
            ? 'Error updating product'
            : 'Error creating product';
          this.notificationService.showError(message);
          this.loading = false;
        },
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.productForm.controls).forEach((field) => {
      const control = this.productForm.get(field);
      control?.markAsTouched({ onlySelf: true });
    });
  }

  onCancel(): void {
    this.router.navigate(['/products']);
  }

  getErrorMessage(fieldName: string): string {
    const field = this.productForm.get(fieldName);
    if (field?.hasError('required')) {
      return `${this.getFieldDisplayName(fieldName)} is required`;
    }
    if (field?.hasError('maxlength')) {
      const maxLength = field.errors?.['maxlength'].requiredLength;
      return `${this.getFieldDisplayName(
        fieldName
      )} cannot exceed ${maxLength} characters`;
    }
    if (field?.hasError('min')) {
      return `${this.getFieldDisplayName(
        fieldName
      )} must be greater than or equal to 0`;
    }
    return '';
  }

  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      name: 'Product Name',
      description: 'Description',
      sku: 'SKU',
      barcode: 'Barcode',
      price: 'Price',
      costPrice: 'Cost Price',
      stockQuantity: 'Stock Quantity',
      minStockLevel: 'Minimum Stock Level',
      categoryId: 'Category',
      supplierId: 'Supplier',
      imageUrl: 'Image URL',
    };
    return displayNames[fieldName] || fieldName;
  }
}
