import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ChartData, ChartOptions } from 'chart.js';

// Angular Material
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';

import { CurrencyFormatPipe } from 'src/app/shared/pipes/currency-format.pipe';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { BaseChartDirective } from 'ng2-charts';

interface SalesData {
  totalSales: number;
  totalRevenue: number;
  averageOrderValue: number;
  topProducts: TopProduct[];
}

interface TopProduct {
  productName: string;
  quantitySold: number;
  revenue: number;
}

@Component({
  selector: 'app-sales-report',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatProgressBarModule,
    MatChipsModule,
    BaseChartDirective,
    CurrencyFormatPipe,
    CommonModule,
    SharedModule,
  ],
  templateUrl: './sales-report.component.html',
  styleUrls: ['./sales-report.component.scss'],
})
export class SalesReportComponent implements OnInit {
  reportForm!: FormGroup;
  loading = false;
  salesData: SalesData | null = null;

  // Chart data
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
    },
  };

  displayedColumns: string[] = ['productName', 'quantitySold', 'revenue'];

  constructor(private formBuilder: FormBuilder) {}

  ngOnInit(): void {
    this.createForm();
    this.generateReport();
  }

  private createForm(): void {
    this.reportForm = this.formBuilder.group({
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      reportType: ['monthly', Validators.required],
    });
  }

  generateReport(): void {
    if (this.reportForm.valid) {
      this.loading = true;

      // Simulate API call with mock data
      setTimeout(() => {
        this.salesData = {
          totalSales: 150,
          totalRevenue: 25000,
          averageOrderValue: 166.67,
          topProducts: [
            { productName: 'Product A', quantitySold: 50, revenue: 10000 },
            { productName: 'Product B', quantitySold: 30, revenue: 7500 },
            { productName: 'Product C', quantitySold: 25, revenue: 5000 },
          ],
        };

        this.setupCharts();
        this.loading = false;
      }, 1000);
    }
  }

  exportReport(): void {
    // Implementation for export functionality
    console.log('Exporting report...');
  }

  private setupCharts(): void {
    // Setup sales chart
    this.salesChartData = {
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May'],
      datasets: [
        {
          label: 'Sales',
          data: [12, 19, 3, 5, 2],
          borderColor: '#3f51b5',
          backgroundColor: 'rgba(63, 81, 181, 0.1)',
        },
      ],
    };

    // Setup revenue chart
    this.revenueChartData = {
      labels: ['Week 1', 'Week 2', 'Week 3', 'Week 4'],
      datasets: [
        {
          label: 'Revenue',
          data: [5000, 7500, 8000, 4500],
          backgroundColor: '#4caf50',
        },
      ],
    };
  }
}
