import { Routes } from '@angular/router';
import { ReportsListComponent } from './reports-list/reports-list';
import { LoadReportsComponent } from './load-reports/load-reports';
import { DriverReportsComponent } from './driver-reports/driver-reports';
import { FinancialReportsComponent } from './financial-reports/financial-reports';
import { DashboardReportsComponent } from './dashboard-reports/dashboard-reports';

export const REPORTS_ROUTES: Routes = [
  {
    path: '',
    component: ReportsListComponent,
    title: 'Reports',
    data: {
      breadcrumb: 'Reports'
    }
  },
  {
    path: 'dashboard',
    component: DashboardReportsComponent,
    title: 'Dashboard Reports',
    data: {
      breadcrumb: 'Dashboard'
    }
  },
  {
    path: 'loads',
    component: LoadReportsComponent,
    title: 'Load Reports',
    data: {
      breadcrumb: 'Load Reports'
    }
  },
  {
    path: 'drivers',
    component: DriverReportsComponent,
    title: 'Driver Reports',
    data: {
      breadcrumb: 'Driver Reports'
    }
  },
  {
    path: 'financial',
    component: FinancialReportsComponent,
    title: 'Financial Reports',
    data: {
      breadcrumb: 'Financial Reports'
    }
  }
];