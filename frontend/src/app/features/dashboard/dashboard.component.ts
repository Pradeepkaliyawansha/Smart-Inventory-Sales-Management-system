import { Component, OnInit, OnDestroy } from '@angular/core';
import { ChartData, ChartOptions } from 'chart.js';
import { DashboardService } from './dashboard.service';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';

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

interface SalesChartResponse {
  dailySales: ChartDataPoint[];
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
export class DashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  loading = true;
  summary: DashboardSummary | null = null;
  topProducts: TopProduct[] = [];
  recentSales: RecentSale[] = [];
  lowStockAlerts: LowStockAlert[] = [];

  // Table column definitions
  displayedSalesColumns: string[] = [
    'invoiceNumber',
    'customerName',
    'totalAmount',
    'saleDate',
  ];
  displayedStockColumns: string[] = [
    'productName',
    'currentStock',
    'minStockLevel',
  ];

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
        beginAtZero: true,
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

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadDashboardData(): void {
    this.loading = true;

    // Use forkJoin instead of Promise.all for better RxJS integration
    forkJoin({
      summary: this.dashboardService.getDashboardSummary(),
      salesChart: this.dashboardService.getSalesChartData(30),
      topProducts: this.dashboardService.getTopProducts(5),
      recentSales: this.dashboardService.getRecentSales(10),
      lowStockAlerts: this.dashboardService.getLowStockAlerts(),
    })
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => (this.loading = false))
      )
      .subscribe({
        next: (data) => {
          this.summary = data.summary;
          this.topProducts = data.topProducts;
          this.recentSales = data.recentSales;
          this.lowStockAlerts = data.lowStockAlerts;

          this.setupSalesChart(data.salesChart.dailySales);
          this.setupTopProductsChart(data.topProducts);
        },
        error: (error) => {
          console.error('Error loading dashboard data:', error);
          // You might want to show a user-friendly error message here
        },
      });
  }

  private setupSalesChart(salesData: ChartDataPoint[]): void {
    if (!salesData || salesData.length === 0) {
      console.warn('No sales data available for chart');
      return;
    }

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
          pointBackgroundColor: '#3f51b5',
          pointBorderColor: '#fff',
          pointBorderWidth: 2,
        },
      ],
    };
  }

  private setupTopProductsChart(products: TopProduct[]): void {
    if (!products || products.length === 0) {
      console.warn('No products data available for chart');
      return;
    }

    const colors = ['#3f51b5', '#e91e63', '#4caf50', '#ff9800', '#9c27b0'];

    this.topProductsChartData = {
      labels: products.map((p) => p.productName),
      datasets: [
        {
          data: products.map((p) => p.totalRevenue),
          backgroundColor: colors.slice(0, products.length),
          borderWidth: 2,
          borderColor: '#fff',
          hoverBackgroundColor: colors
            .slice(0, products.length)
            .map((color) => color + 'CC'),
        },
      ],
    };
  }

  formatCurrency(value: number | null | undefined): string {
    if (value == null) return '$0.00';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(value);
  }

  formatDate(date: Date | string | null | undefined): string {
    if (!date) return 'N/A';

    try {
      return new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
      });
    } catch (error) {
      console.error('Error formatting date:', error);
      return 'Invalid Date';
    }
  }

  getStockStatusColor(
    currentStock: number,
    minStock: number
  ): 'primary' | 'accent' | 'warn' {
    if (currentStock === 0) return 'warn';
    if (currentStock <= minStock) return 'accent';
    return 'primary';
  }

  refreshDashboard(): void {
    this.loadDashboardData();
  }
}
