import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

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

// Chart.js
import { NgChartsModule } from 'ng2-charts';

// Components
import { SalesReportComponent } from './sales-report/sales-report.component';

// Shared Module
import { SharedModule } from '../../shared/shared.module';
import { InventoryReportComponent } from './inventory-report/inventory-report.component';
import { MatChipsModule } from '@angular/material/chips';

const routes = [
  {
    path: '',
    redirectTo: 'sales',
    pathMatch: 'full',
  },
  {
    path: 'sales',
    component: SalesReportComponent,
  },
];

@NgModule({
  declarations: [SalesReportComponent, InventoryReportComponent],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule,
    SharedModule,

    // Angular Material
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

    // Chart.js
    NgChartsModule,
  ],
})
export class ReportsModule {}
