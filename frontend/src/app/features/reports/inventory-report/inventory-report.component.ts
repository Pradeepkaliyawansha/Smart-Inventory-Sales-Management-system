import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';

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
