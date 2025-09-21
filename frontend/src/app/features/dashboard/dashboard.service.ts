import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

interface DashboardSummary {
  totalSalesAmount: number;
  totalSalesAmountToday: number;
  totalProducts: number;
  lowStockProductsCount: number;
  totalCustomers: number;
  totalSalesCount: number;
  totalSalesCountToday: number;
  averageOrderValue: number;
}

interface SalesChartData {
  dailySales: ChartDataPoint[];
  monthlySales: ChartDataPoint[];
}

interface ChartDataPoint {
  label: string;
  value: number;
}

interface TopProduct {
  productId: number;
  productName: string;
  totalQuantitySold: number;
  totalRevenue: number;
}

interface RecentSale {
  id: number;
  invoiceNumber: string;
  customerName: string;
  totalAmount: number;
  saleDate: Date;
}

interface LowStockAlert {
  productId: number;
  productName: string;
  currentStock: number;
  minStockLevel: number;
  categoryName: string;
}

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  constructor(private http: HttpClient) {}

  getDashboardSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(
      `${environment.apiUrl}/dashboard/summary`
    );
  }

  getSalesChartData(days: number = 30): Observable<SalesChartData> {
    return this.http.get<SalesChartData>(
      `${environment.apiUrl}/dashboard/sales-chart`,
      {
        params: { days: days.toString() },
      }
    );
  }

  getTopProducts(count: number = 10): Observable<TopProduct[]> {
    return this.http.get<TopProduct[]>(
      `${environment.apiUrl}/dashboard/top-products`,
      {
        params: { count: count.toString() },
      }
    );
  }

  getRecentSales(count: number = 10): Observable<RecentSale[]> {
    return this.http.get<RecentSale[]>(
      `${environment.apiUrl}/dashboard/recent-sales`,
      {
        params: { count: count.toString() },
      }
    );
  }

  getLowStockAlerts(): Observable<LowStockAlert[]> {
    return this.http.get<LowStockAlert[]>(
      `${environment.apiUrl}/dashboard/low-stock-alerts`
    );
  }
}
