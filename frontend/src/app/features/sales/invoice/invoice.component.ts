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
  currentDate: Date = new Date();
  dueDate: Date = new Date();

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

  downloadInvoice(): void {
    // Simple approach: export the HTML content as PDF using browser print dialog
    // For production, better use libraries like jspdf or pdfmake
    const content = document.querySelector('.invoice-container')?.innerHTML;
    if (content) {
      const newWindow = window.open('', '', 'width=800,height=600');
      if (newWindow) {
        newWindow.document.write(
          '<html><head><title>Invoice</title></head><body>'
        );
        newWindow.document.write(content);
        newWindow.document.write('</body></html>');
        newWindow.document.close();
        newWindow.print();
      }
    }
  }
}
