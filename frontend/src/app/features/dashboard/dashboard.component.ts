import { Component, OnInit } from '@angular/core';
import { ChartData, ChartOptions, ChartType } from 'chart.js';
import { DashboardService } from './dashboard.service';

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

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  loading = true;
  summary: DashboardSummary | null = null;
  topProducts: TopProduct[] = [];
  recentSales: RecentSale[] = [];
  lowStockAlerts: LowStockAlert[] = [];

  // Chart configurations
  salesChartData: ChartData<'line'> = {
    labels: [],
    datasets: [],
  };

  salesChartOptions: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'top',
      },
      title: {
        display: true,
        text: 'Sales Trend (Last 30 Days)',
      },
    },
    scales: {
      x: {
        display: true,
        title: {
          display: true,
          text: 'Date',
        },
      },
      y: {
        display: true,
        title: {
          display: true,
          text: 'Sales Amount ($)',
        },
      },
    },
  };

  topProductsChartData: ChartData<'doughnut'> = {
    labels: [],
    datasets: [],
  };

  topProductsChartOptions: ChartOptions<'doughnut'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'right',
      },
      title: {
        display: true,
        text: 'Top 5 Products by Revenue',
      },
    },
  };

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  async loadDashboardData(): Promise<void> {
    try {
      this.loading = true;

      // Load all dashboard data concurrently
      const [summary, salesChart, topProducts, recentSales, lowStockAlerts] =
        await Promise.all([
          this.dashboardService.getDashboardSummary().toPromise(),
          this.dashboardService.getSalesChartData(30).toPromise(),
          this.dashboardService.getTopProducts(5).toPromise(),
          this.dashboardService.getRecentSales(10).toPromise(),
          this.dashboardService.getLowStockAlerts().toPromise(),
        ]);

      this.summary = summary!;
      this.topProducts = topProducts!;
      this.recentSales = recentSales!;
      this.lowStockAlerts = lowStockAlerts!;

      this.setupSalesChart(salesChart!.dailySales);
      this.setupTopProductsChart(topProducts!);
    } catch (error) {
      console.error('Error loading dashboard data:', error);
    } finally {
      this.loading = false;
    }
  }

  private setupSalesChart(salesData: ChartDataPoint[]): void {
    this.salesChartData = {
      labels: salesData.map((item) => item.label),
      datasets: [
        {
          label: 'Daily Sales',
          data: salesData.map((item) => item.value),
          borderColor: '#3f51b5',
          backgroundColor: 'rgba(63, 81, 181, 0.1)',
          fill: true,
          tension: 0.4,
        },
      ],
    };
  }

  private setupTopProductsChart(products: TopProduct[]): void {
    const colors = ['#3f51b5', '#e91e63', '#4caf50', '#ff9800', '#9c27b0'];

    this.topProductsChartData = {
      labels: products.map((p) => p.productName),
      datasets: [
        {
          data: products.map((p) => p.totalRevenue),
          backgroundColor: colors.slice(0, products.length),
          borderWidth: 2,
          borderColor: '#fff',
        },
      ],
    };
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(value);
  }

  formatDate(date: Date | string): string {
    return new Date(date).toLocaleDateString();
  }

  getStockStatusColor(currentStock: number, minStock: number): string {
    if (currentStock === 0) return 'warn';
    if (currentStock <= minStock) return 'accent';
    return 'primary';
  }

  refreshDashboard(): void {
    this.loadDashboardData();
  }
}
