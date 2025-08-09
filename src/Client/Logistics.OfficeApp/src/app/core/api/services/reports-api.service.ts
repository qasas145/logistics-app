import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiBase } from '../api-base';

// Report DTOs interfaces
export interface LoadReportDto {
  id: string;
  number: number;
  name: string;
  type: string;
  status: string;
  originAddress: string;
  destinationAddress: string;
  distance: number;
  deliveryCost: number;
  currency: string;
  driverShare: number;
  companyRevenue: number;
  dispatchedDate: string;
  pickUpDate?: string;
  deliveryDate?: string;
  deliveryTimeInHours?: number;
  assignedTruckNumber?: string;
  assignedDriverName?: string;
  assignedDispatcherName?: string;
  customerName?: string;
  hasInvoice: boolean;
  invoiceStatus?: string;
  invoiceTotal?: number;
  invoiceDueDate?: string;
  isPaid: boolean;
}

export interface DriverReportDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  joinedDate: string;
  currentTruckNumber?: string;
  truckModel?: string;
  truckStatus?: string;
  totalLoadsCompleted: number;
  totalLoadsInProgress: number;
  totalLoadsDispatched: number;
  totalDistanceDriven: number;
  averageDistancePerLoad: number;
  totalEarnings: number;
  averageEarningsPerLoad: number;
  averageEarningsPerKm: number;
  onTimeDeliveryCount: number;
  lateDeliveryCount: number;
  onTimeDeliveryPercentage: number;
  averageDeliveryTimeInHours: number;
  thisWeek: DriverPeriodStatsDto;
  lastWeek: DriverPeriodStatsDto;
  thisMonth: DriverPeriodStatsDto;
  lastMonth: DriverPeriodStatsDto;
  thisYear: DriverPeriodStatsDto;
  recentLoads: LoadReportDto[];
  lastActiveDate?: string;
  lastKnownLocation?: string;
}

export interface DriverPeriodStatsDto {
  loadsCompleted: number;
  totalEarnings: number;
  totalRevenue: number;
  totalDistance: number;
  averageEarningsPerLoad: number;
  onTimeDeliveries: number;
  lateDeliveries: number;
  onTimePercentage: number;
}

export interface FinancialReportDto {
  reportDate: string;
  reportPeriod: string;
  periodStart: string;
  periodEnd: string;
  revenue: RevenueBreakdownDto;
  expenses: ExpenseBreakdownDto;
  grossProfit: number;
  netProfit: number;
  profitMargin: number;
  paymentStatus: PaymentStatusDto;
  invoiceSummary: InvoiceSummaryDto;
  topDrivers: TopPerformerDto[];
  topCustomers: TopCustomerDto[];
  comparison: FinancialComparisonDto;
}

export interface RevenueBreakdownDto {
  totalRevenue: number;
  loadRevenue: number;
  fuelSurcharges: number;
  accessorialCharges: number;
  totalLoadsDelivered: number;
  averageRevenuePerLoad: number;
  averageRevenuePerMile: number;
  revenueByLoadType: RevenueByTypeDto[];
}

export interface ExpenseBreakdownDto {
  totalExpenses: number;
  driverPayouts: number;
  fuelCosts: number;
  maintenanceCosts: number;
  insuranceCosts: number;
  operationalExpenses: number;
  payrollExpenses: number;
  expensesByCategory: ExpenseByTypeDto[];
}

export interface PaymentStatusDto {
  totalReceivables: number;
  paidAmount: number;
  pendingAmount: number;
  overdueAmount: number;
  totalInvoices: number;
  paidInvoices: number;
  pendingInvoices: number;
  overdueInvoices: number;
  collectionPercentage: number;
  averageDaysToPayment: number;
}

export interface InvoiceSummaryDto {
  totalInvoicesIssued: number;
  totalInvoiceValue: number;
  averageInvoiceValue: number;
  invoicesPaid: number;
  invoicesPending: number;
  invoicesOverdue: number;
  invoiceAging: InvoiceAgingDto[];
}

export interface TopPerformerDto {
  id: string;
  name: string;
  totalEarnings: number;
  loadsCompleted: number;
  totalDistance: number;
  averagePerLoad: number;
}

export interface TopCustomerDto {
  id: string;
  name: string;
  totalRevenue: number;
  totalLoads: number;
  averageLoadValue: number;
  outstandingBalance: number;
}

export interface FinancialComparisonDto {
  previousPeriodRevenue: number;
  revenueGrowth: number;
  revenueGrowthPercentage: number;
  previousPeriodProfit: number;
  profitGrowth: number;
  profitGrowthPercentage: number;
  previousPeriodLoads: number;
  loadGrowth: number;
  loadGrowthPercentage: number;
}

export interface RevenueByTypeDto {
  loadType: string;
  revenue: number;
  loadCount: number;
  percentage: number;
}

export interface ExpenseByTypeDto {
  category: string;
  amount: number;
  percentage: number;
}

export interface InvoiceAgingDto {
  ageRange: string;
  count: number;
  amount: number;
  percentage: number;
}

// Summary DTOs
export interface LoadReportSummaryDto {
  totalLoads: number;
  completedLoads: number;
  inProgressLoads: number;
  dispatchedLoads: number;
  totalRevenue: number;
  totalDriverPayouts: number;
  totalDistance: number;
  averageDeliveryCost: number;
  averageDistance: number;
  averageDeliveryTime: number;
  onTimeDeliveryPercentage: number;
  loadsByStatus: LoadsByStatusDto[];
  loadsByType: LoadsByTypeDto[];
}

export interface LoadsByStatusDto {
  status: string;
  count: number;
  revenue: number;
  percentage: number;
}

export interface LoadsByTypeDto {
  type: string;
  count: number;
  revenue: number;
  percentage: number;
}

export interface DriverReportSummaryDto {
  totalDrivers: number;
  activeDrivers: number;
  inactiveDrivers: number;
  totalDriverEarnings: number;
  averageDriverEarnings: number;
  totalDistanceDriven: number;
  averageDistancePerDriver: number;
  totalLoadsCompleted: number;
  averageLoadsPerDriver: number;
  overallOnTimePercentage: number;
  topPerformers: TopDriverDto[];
  driverEfficiency: DriverEfficiencyDto[];
}

export interface TopDriverDto {
  id: string;
  name: string;
  totalEarnings: number;
  loadsCompleted: number;
  totalDistance: number;
  onTimePercentage: number;
}

export interface DriverEfficiencyDto {
  efficiencyRange: string;
  driverCount: number;
  averageEarnings: number;
  averageOnTimePercentage: number;
}

export interface FinancialSummaryDto {
  totalRevenue: number;
  totalExpenses: number;
  grossProfit: number;
  netProfit: number;
  profitMargin: number;
  totalInvoices: number;
  paidInvoices: number;
  overdueInvoices: number;
  outstandingBalance: number;
  collectionRate: number;
  monthlyTrends: MonthlyFinancialTrendDto[];
}

export interface MonthlyFinancialTrendDto {
  month: string;
  revenue: number;
  expenses: number;
  profit: number;
  loadsCompleted: number;
}

export interface DashboardReportDto {
  reportDate: string;
  periodStart: string;
  periodEnd: string;
  loadSummary: LoadReportSummaryDto;
  driverSummary: DriverReportSummaryDto;
  financialSummary: FinancialSummaryDto;
}

// Query interfaces
export interface GetLoadReportQuery {
  startDate?: string;
  endDate?: string;
  status?: string;
  loadType?: string;
  assignedDriverId?: string;
  assignedTruckId?: string;
  customerId?: string;
  minDeliveryCost?: number;
  maxDeliveryCost?: number;
  includeInvoiceDetails?: boolean;
  sortBy?: string;
  sortOrder?: string;
  page?: number;
  pageSize?: number;
}

export interface GetDriverReportQuery {
  startDate?: string;
  endDate?: string;
  driverId?: string;
  truckId?: string;
  status?: string;
  includeRecentLoads?: boolean;
  recentLoadsLimit?: number;
  sortBy?: string;
  sortOrder?: string;
  page?: number;
  pageSize?: number;
}

export interface GetFinancialReportQuery {
  startDate: string;
  endDate: string;
  reportType?: string;
  period?: string;
  includeComparison?: boolean;
  includeTopPerformers?: boolean;
  topPerformersLimit?: number;
  includeInvoiceAging?: boolean;
  currency?: string;
}

export interface DashboardReportQuery {
  startDate?: string;
  endDate?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ReportsApiService extends ApiBase {

  // Load Reports
  getLoadReports(query: GetLoadReportQuery): Observable<any> {
    return this.get('reports/loads', query);
  }

  getLoadReportSummary(query: Partial<GetLoadReportQuery>): Observable<any> {
    return this.get('reports/loads/summary', query);
  }

  // Driver Reports
  getDriverReports(query: GetDriverReportQuery): Observable<any> {
    return this.get('reports/drivers', query);
  }

  getDriverReport(driverId: string, query?: Partial<GetDriverReportQuery>): Observable<any> {
    return this.get(`reports/drivers/${driverId}`, query);
  }

  getDriverReportSummary(query?: Partial<GetDriverReportQuery>): Observable<any> {
    return this.get('reports/drivers/summary', query);
  }

  // Financial Reports
  getFinancialReport(query: GetFinancialReportQuery): Observable<any> {
    return this.get('reports/financial', query);
  }

  getFinancialSummary(query?: Partial<GetFinancialReportQuery>): Observable<any> {
    return this.get('reports/financial/summary', query);
  }

  getCashFlowReport(query: { startDate: string; endDate: string; period?: string }): Observable<any> {
    return this.get('reports/financial/cash-flow', query);
  }

  // Dashboard Reports
  getDashboardReport(query?: DashboardReportQuery): Observable<any> {
    return this.get('reports/dashboard', query);
  }

  // Export functionality
  exportLoadReportToPdf(query: GetLoadReportQuery): Observable<Blob> {
    return this.getBlob('reports/loads/export/pdf', query);
  }

  exportLoadReportToExcel(query: GetLoadReportQuery): Observable<Blob> {
    return this.getBlob('reports/loads/export/excel', query);
  }

  exportDriverReportToPdf(query: GetDriverReportQuery): Observable<Blob> {
    return this.getBlob('reports/drivers/export/pdf', query);
  }

  exportDriverReportToExcel(query: GetDriverReportQuery): Observable<Blob> {
    return this.getBlob('reports/drivers/export/excel', query);
  }

  exportFinancialReportToPdf(query: GetFinancialReportQuery): Observable<Blob> {
    return this.getBlob('reports/financial/export/pdf', query);
  }

  exportFinancialReportToExcel(query: GetFinancialReportQuery): Observable<Blob> {
    return this.getBlob('reports/financial/export/excel', query);
  }

  exportDashboardReportToPdf(query?: DashboardReportQuery): Observable<Blob> {
    return this.getBlob('reports/dashboard/export/pdf', query);
  }

  exportDashboardReportToExcel(query?: DashboardReportQuery): Observable<Blob> {
    return this.getBlob('reports/dashboard/export/excel', query);
  }
}