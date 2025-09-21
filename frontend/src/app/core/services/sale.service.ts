import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Sale, CreateSaleRequest } from '../models/sale.model';

@Injectable({
  providedIn: 'root',
})
export class SaleService {
  constructor(private http: HttpClient) {}

  getSales(): Observable<Sale[]> {
    return this.http.get<Sale[]>(`${environment.apiUrl}/sales`);
  }

  getSale(id: number): Observable<Sale> {
    return this.http.get<Sale>(`${environment.apiUrl}/sales/${id}`);
  }

  createSale(sale: CreateSaleRequest): Observable<Sale> {
    return this.http.post<Sale>(`${environment.apiUrl}/sales`, sale);
  }

  getSalesByDateRange(startDate: Date, endDate: Date): Observable<Sale[]> {
    return this.http.get<Sale[]>(`${environment.apiUrl}/sales/date-range`, {
      params: {
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString(),
      },
    });
  }

  getTotalSalesAmount(
    startDate?: Date,
    endDate?: Date
  ): Observable<{ totalAmount: number }> {
    const params: any = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();

    return this.http.get<{ totalAmount: number }>(
      `${environment.apiUrl}/sales/total-amount`,
      { params }
    );
  }

  getInvoice(saleId: number): Observable<any> {
    return this.http.get(`${environment.apiUrl}/sales/${saleId}/invoice`);
  }
}
