import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { Product } from '../../../core/models/product.model';
import { ProductService } from '../../../core/services/product.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmationDialogComponent } from '../../../shared/components/confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss'],
})
export class ProductListComponent implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns: string[] = [
    'name',
    'sku',
    'price',
    'stockQuantity',
    'categoryName',
    'status',
    'actions',
  ];
  dataSource = new MatTableDataSource<Product>();
  loading = false;

  constructor(
    private productService: ProductService,
    private notificationService: NotificationService,
    private dialog: MatDialog,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadProducts(): void {
    this.loading = true;
    this.productService.getProducts().subscribe({
      next: (products) => {
        this.dataSource.data = products;
        this.loading = false;
      },
      error: (error) => {
        this.notificationService.showError('Error loading products');
        this.loading = false;
        console.error('Error loading products:', error);
      },
    });
  }

  applyFilter(event: Event): void {
    const target = event.target as HTMLInputElement;
    const filterValue = target.value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  editProduct(product: Product): void {
    this.router.navigate(['/products/edit', product.id]);
  }

  viewProduct(product: Product): void {
    this.router.navigate(['/products', product.id]);
  }

  deleteProduct(product: Product): void {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Product',
        message: `Are you sure you want to delete "${product.name}"?`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
        type: 'error',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.productService.deleteProduct(product.id).subscribe({
          next: () => {
            this.notificationService.showSuccess(
              'Product deleted successfully'
            );
            this.loadProducts();
          },
          error: (error) => {
            this.notificationService.showError('Error deleting product');
            console.error('Error deleting product:', error);
          },
        });
      }
    });
  }

  getStockStatus(product: Product): string {
    if (product.stockQuantity === 0) return 'out-of-stock';
    if (product.isLowStock) return 'low-stock';
    return 'in-stock';
  }

  getStockStatusText(product: Product): string {
    if (product.stockQuantity === 0) return 'Out of Stock';
    if (product.isLowStock) return 'Low Stock';
    return 'In Stock';
  }
}
