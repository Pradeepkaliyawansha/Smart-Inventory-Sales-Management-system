import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';

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
interface InventoryData {
  totalProducts: number;
  totalValue: number;
  lowStockItems: number;
  outOfStockItems: number;
  topCategories: Array<{
    name: string;
    productCount: number;
    totalValue: number;
  }>;
  stockMovements: Array<{
    productName: string;
    movement: 'in' | 'out';
    quantity: number;
    date: Date;
    reference: string;
  }>;
}

@Component({
  selector: 'app-inventory-report',
  standalone: true,
  imports: [
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
    CurrencyFormatPipe,
    CommonModule,
    SharedModule,
  ],
  templateUrl: './inventory-report.component.html',
  styleUrls: ['./inventory-report.component.scss'],
})
export class InventoryReportComponent implements OnInit {
  reportForm!: FormGroup;
  loading = false;
  inventoryData: InventoryData | null = null;

  displayedColumns: string[] = [
    'productName',
    'movement',
    'quantity',
    'date',
    'reference',
  ];

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
      category: ['all'],
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
        this.inventoryData = {
          totalProducts: 245,
          totalValue: 48750.0,
          lowStockItems: 12,
          outOfStockItems: 3,
          topCategories: [
            { name: 'Electronics', productCount: 85, totalValue: 25500 },
            { name: 'Clothing', productCount: 92, totalValue: 15800 },
            { name: 'Books', productCount: 68, totalValue: 7450 },
          ],
          stockMovements: [
            {
              productName: 'Product A',
              movement: 'out',
              quantity: 5,
              date: new Date(),
              reference: 'Sale #001',
            },
            {
              productName: 'Product B',
              movement: 'in',
              quantity: 20,
              date: new Date(),
              reference: 'Purchase #123',
            },
            {
              productName: 'Product C',
              movement: 'out',
              quantity: 3,
              date: new Date(),
              reference: 'Sale #002',
            },
          ],
        };

        this.loading = false;
      }, 1000);
    }
  }

  exportReport(): void {
    console.log('Exporting inventory report...');
  }
}
