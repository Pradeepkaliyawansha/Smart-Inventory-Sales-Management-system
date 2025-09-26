import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full',
  },
  {
    path: 'auth',
    loadChildren: () =>
      import('./features/auth/auth.module').then((m) => m.AuthModule),
  },
  {
    path: 'dashboard',
    loadChildren: () =>
      import('./features/dashboard/dashboard.module').then(
        (m) => m.DashboardModule
      ),
    canActivate: [AuthGuard],
  },
  {
    path: 'products',
    loadChildren: () =>
      import('./features/products/products.module').then(
        (m) => m.ProductsModule
      ),
    canActivate: [AuthGuard],
  },
  {
    path: 'sales',
    loadChildren: () =>
      import('./features/sales/sales.module').then((m) => m.SalesModule),
    canActivate: [AuthGuard],
  },
  {
    path: 'customers',
    loadChildren: () =>
      import('./features/customers/customers.module').then(
        (m) => m.CustomersModule
      ),
    canActivate: [AuthGuard],
  },
  {
    path: 'reports',
    loadChildren: () =>
      import('./features/reports/reports.module').then((m) => m.ReportsModule),
    canActivate: [AuthGuard],
  },
  {
    path: '**',
    redirectTo: '/dashboard',
  },
];
