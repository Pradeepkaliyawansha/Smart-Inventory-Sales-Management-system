import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { Router } from '@angular/router';
import { Sale, PaymentMethod } from '../../../core/models/sale.model';
import { SaleService } from '../../../core/services/sale.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-sale-list',
  templateUrl: './sale-list.component.html',
  styleUrls: ['./sale-list.component.scss'],
})
export class SaleListComponent implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns: string[] = [
    'invoiceNumber',
    'customerName',
    'saleDate',
    'totalAmount',
    'paymentMethod',
    'status',
    'actions',
  ];
  dataSource = new MatTableDataSource<Sale>();
  loading = false;

  constructor(
    private saleService: SaleService,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadSales();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadSales(): void {
    this.loading = true;
    this.saleService.getSales().subscribe({
      next: (sales) => {
        this.dataSource.data = sales;
        this.loading = false;
      },
      error: () => {
        this.notificationService.showError('Error loading sales');
        this.loading = false;
      },
    });
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  viewInvoice(sale: Sale): void {
    this.router.navigate(['/sales', sale.id, 'invoice']);
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
