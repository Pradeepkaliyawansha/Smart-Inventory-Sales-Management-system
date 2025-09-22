import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ChartData, ChartOptions } from 'chart.js';

interface SalesData {
  period: string;
  totalSales: number;
  totalRevenue: number;
  averageOrderValue: number;
  topProducts: Array<{
    name: string;
    quantity: number;
    revenue: number;
  }>;
}

@Component({
  selector: 'app-sales-report',
  templateUrl: './sales-report.component.html',
  styleUrls: ['./sales-report.component.scss'],
})
export class SalesReportComponent implements OnInit {
  reportForm!: FormGroup;
  loading = false;
  salesData: SalesData | null = null;

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
        text: 'Sales Trend',
      },
    },
  };

  revenueChartData: ChartData<'bar'> = {
    labels: [],
    datasets: [],
  };

  revenueChartOptions: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'top',
      },
      title: {
        display: true,
        text: 'Revenue by Period',
      },
    },
  };

  constructor(private formBuilder: FormBuilder) {}

  ngOnInit(): void {
    this.createForm();
    this.loadDefaultReport();
  }

  private createForm(): void {
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(endDate.getDate() - 30);

    this.reportForm = this.formBuilder.group({
      startDate: [startDate],
      endDate: [endDate],
      reportType: ['daily'],
    });
  }

  private loadDefaultReport(): void {
    this.generateReport();
  }

  generateReport(): void {
    if (this.reportForm.valid) {
      this.loading = true;

      // Simulate API call
      setTimeout(() => {
        this.salesData = {
          period: '30 Days',
          totalSales: 142,
          totalRevenue: 18450.5,
          averageOrderValue: 129.93,
          topProducts: [
            { name: 'Product A', quantity: 45, revenue: 4500 },
            { name: 'Product B', quantity: 32, revenue: 3200 },
            { name: 'Product C', quantity: 28, revenue: 2800 },
          ],
        };

        this.setupCharts();
        this.loading = false;
      }, 1000);
    }
  }

  private setupCharts(): void {
    // Sales trend chart
    this.salesChartData = {
      labels: ['Week 1', 'Week 2', 'Week 3', 'Week 4'],
      datasets: [
        {
          label: 'Sales Count',
          data: [35, 42, 38, 27],
          borderColor: '#3f51b5',
          backgroundColor: 'rgba(63, 81, 181, 0.1)',
          fill: true,
        },
      ],
    };

    // Revenue chart
    this.revenueChartData = {
      labels: ['Week 1', 'Week 2', 'Week 3', 'Week 4'],
      datasets: [
        {
          label: 'Revenue',
          data: [4200, 5100, 4800, 4350],
          backgroundColor: '#4caf50',
          borderColor: '#388e3c',
          borderWidth: 1,
        },
      ],
    };
  }

  exportReport(): void {
    // Implement export functionality
    console.log('Exporting sales report...');
  }
}
