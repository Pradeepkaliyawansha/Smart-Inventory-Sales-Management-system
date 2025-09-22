import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { SaleService } from '../../../core/services/sale.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Sale, PaymentMethod } from '../../../core/models/sale.model';

@Component({
  selector: 'app-invoice',
  templateUrl: './invoice.component.html',
  styleUrls: ['./invoice.component.scss'],
})
export class InvoiceComponent implements OnInit {
  sale: Sale | null = null;
  loading = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private saleService: SaleService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadSale(+id);
    } else {
      this.router.navigate(['/sales']);
    }
  }

  loadSale(id: number): void {
    this.loading = true;
    this.saleService.getSale(id).subscribe({
      next: (sale) => {
        this.sale = sale;
        this.loading = false;
      },
      error: () => {
        this.notificationService.showError('Error loading sale details');
        this.router.navigate(['/sales']);
      },
    });
  }

  printInvoice(): void {
    window.print();
  }

  goBack(): void {
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
