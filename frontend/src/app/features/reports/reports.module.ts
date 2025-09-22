import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

// Angular Material
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';

// Chart.js
import { NgChartsModule } from 'ng2-charts';

// Components
import { SalesReportComponent } from './sales-report/sales-report.component';
import { InventoryReportComponent } from './inventory-report/inventory-report.component';
import { ReportsOverviewComponent } from './reports-overview/reports-overview.component';

// Shared Module
import { SharedModule } from '../../shared/shared.module';

const routes = [
  {
    path: '',
    component: ReportsOverviewComponent,
  },
  {
    path: 'sales',
    component: SalesReportComponent,
  },
  {
    path: 'inventory',
    component: InventoryReportComponent,
  },
];

@NgModule({
  declarations: [
    SalesReportComponent,
    InventoryReportComponent,
    ReportsOverviewComponent,
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule.forChild(routes),
    SharedModule,

    // Angular Material
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatSelectModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatTabsModule,

    // Chart.js
    NgChartsModule,
  ],
})
export class ReportsModule {}
