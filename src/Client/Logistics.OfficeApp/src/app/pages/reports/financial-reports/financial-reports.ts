import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { 
  ReportsApiService, 
  FinancialReportDto,
  FinancialSummaryDto,
  GetFinancialReportQuery 
} from '../../../core/api/services/reports-api.service';

@Component({
  selector: 'app-financial-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container-fluid p-4">
      <!-- Header -->
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 class="mb-1">Financial Reports</h2>
          <p class="text-muted mb-0">Revenue analysis, cash flow, and profitability insights</p>
        </div>
        <div class="d-flex gap-2">
          <button class="btn btn-outline-primary" (click)="refreshData()">
            <i class="fas fa-refresh me-2"></i>Refresh
          </button>
          <div class="btn-group">
            <button class="btn btn-success dropdown-toggle" data-bs-toggle="dropdown">
              <i class="fas fa-download me-2"></i>Export
            </button>
            <ul class="dropdown-menu">
              <li><a class="dropdown-item" (click)="exportToPdf()">
                <i class="fas fa-file-pdf me-2"></i>Export as PDF
              </a></li>
              <li><a class="dropdown-item" (click)="exportToExcel()">
                <i class="fas fa-file-excel me-2"></i>Export as Excel
              </a></li>
            </ul>
          </div>
        </div>
      </div>

      <!-- Filters -->
      <div class="card mb-4">
        <div class="card-header">
          <h5 class="mb-0">Report Configuration</h5>
        </div>
        <div class="card-body">
          <div class="row g-3">
            <div class="col-md-3">
              <label class="form-label">Start Date *</label>
              <input type="date" class="form-control" [(ngModel)]="filters.startDate" required>
            </div>
            <div class="col-md-3">
              <label class="form-label">End Date *</label>
              <input type="date" class="form-control" [(ngModel)]="filters.endDate" required>
            </div>
            <div class="col-md-2">
              <label class="form-label">Report Type</label>
              <select class="form-control" [(ngModel)]="filters.reportType">
                <option value="">Standard</option>
                <option value="detailed">Detailed</option>
                <option value="summary">Summary</option>
              </select>
            </div>
            <div class="col-md-2">
              <label class="form-label">Period</label>
              <select class="form-control" [(ngModel)]="filters.period">
                <option value="custom">Custom</option>
                <option value="monthly">Monthly</option>
                <option value="quarterly">Quarterly</option>
                <option value="yearly">Yearly</option>
              </select>
            </div>
            <div class="col-md-2">
              <button class="btn btn-primary" (click)="generateReport()" [disabled]="!filters.startDate || !filters.endDate">
                <i class="fas fa-chart-line me-2"></i>Generate Report
              </button>
            </div>
          </div>
          
          <!-- Options -->
          <div class="row g-3 mt-2">
            <div class="col-12">
              <div class="form-check form-check-inline">
                <input class="form-check-input" type="checkbox" [(ngModel)]="filters.includeComparison" id="includeComparison">
                <label class="form-check-label" for="includeComparison">Include Period Comparison</label>
              </div>
              <div class="form-check form-check-inline">
                <input class="form-check-input" type="checkbox" [(ngModel)]="filters.includeTopPerformers" id="includeTopPerformers">
                <label class="form-check-label" for="includeTopPerformers">Include Top Performers</label>
              </div>
              <div class="form-check form-check-inline">
                <input class="form-check-input" type="checkbox" [(ngModel)]="filters.includeInvoiceAging" id="includeInvoiceAging">
                <label class="form-check-label" for="includeInvoiceAging">Include Invoice Aging</label>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div *ngIf="isLoading()" class="text-center py-5">
        <div class="spinner-border" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
      </div>

      <div *ngIf="!isLoading() && report()">
        <!-- Financial Summary Cards -->
        <div class="row g-4 mb-4">
          <div class="col-xl-3 col-md-6">
            <div class="card border-success">
              <div class="card-body text-center">
                <i class="fas fa-chart-line fa-2x text-success mb-3"></i>
                <h4 class="mb-1">{{ report()!.revenue.totalRevenue | currency }}</h4>
                <p class="text-muted mb-0">Total Revenue</p>
                <small class="text-muted">{{ report()!.revenue.totalLoadsDelivered }} loads delivered</small>
              </div>
            </div>
          </div>
          <div class="col-xl-3 col-md-6">
            <div class="card border-danger">
              <div class="card-body text-center">
                <i class="fas fa-chart-bar fa-2x text-danger mb-3"></i>
                <h4 class="mb-1">{{ report()!.expenses.totalExpenses | currency }}</h4>
                <p class="text-muted mb-0">Total Expenses</p>
                <small class="text-muted">Driver payouts: {{ report()!.expenses.driverPayouts | currency }}</small>
              </div>
            </div>
          </div>
          <div class="col-xl-3 col-md-6">
            <div class="card border-info">
              <div class="card-body text-center">
                <i class="fas fa-dollar-sign fa-2x text-info mb-3"></i>
                <h4 class="mb-1" [class]="report()!.grossProfit >= 0 ? 'text-success' : 'text-danger'">
                  {{ report()!.grossProfit | currency }}
                </h4>
                <p class="text-muted mb-0">Gross Profit</p>
                <small class="text-muted">Margin: {{ report()!.profitMargin | number:'1.1-1' }}%</small>
              </div>
            </div>
          </div>
          <div class="col-xl-3 col-md-6">
            <div class="card border-warning">
              <div class="card-body text-center">
                <i class="fas fa-percentage fa-2x text-warning mb-3"></i>
                <h4 class="mb-1">{{ report()!.paymentStatus.collectionPercentage | number:'1.1-1' }}%</h4>
                <p class="text-muted mb-0">Collection Rate</p>
                <small class="text-muted">{{ report()!.paymentStatus.averageDaysToPayment | number:'1.0-0' }} days avg</small>
              </div>
            </div>
          </div>
        </div>

        <!-- Detailed Financial Analysis -->
        <div class="row g-4 mb-4">
          <!-- Revenue Breakdown -->
          <div class="col-xl-6">
            <div class="card">
              <div class="card-header">
                <h5 class="mb-0">Revenue Breakdown</h5>
              </div>
              <div class="card-body">
                <div class="d-flex justify-content-between mb-2">
                  <span>Load Revenue:</span>
                  <strong>{{ report()!.revenue.loadRevenue | currency }}</strong>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span>Fuel Surcharges:</span>
                  <span>{{ report()!.revenue.fuelSurcharges | currency }}</span>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span>Accessorial Charges:</span>
                  <span>{{ report()!.revenue.accessorialCharges | currency }}</span>
                </div>
                <hr>
                <div class="d-flex justify-content-between mb-2">
                  <span>Average per Load:</span>
                  <span>{{ report()!.revenue.averageRevenuePerLoad | currency }}</span>
                </div>
                <div class="d-flex justify-content-between">
                  <span>Average per Mile:</span>
                  <span>{{ report()!.revenue.averageRevenuePerMile | currency }}</span>
                </div>
                
                <!-- Revenue by Load Type -->
                <div class="mt-3" *ngIf="report()!.revenue.revenueByLoadType.length > 0">
                  <h6>Revenue by Load Type</h6>
                  <div *ngFor="let type of report()!.revenue.revenueByLoadType" class="d-flex justify-content-between mb-1">
                    <span>{{ type.loadType }}:</span>
                    <span>{{ type.revenue | currency }} ({{ type.percentage | number:'1.1-1' }}%)</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Expense Breakdown -->
          <div class="col-xl-6">
            <div class="card">
              <div class="card-header">
                <h5 class="mb-0">Expense Breakdown</h5>
              </div>
              <div class="card-body">
                <div class="d-flex justify-content-between mb-2">
                  <span>Driver Payouts:</span>
                  <strong>{{ report()!.expenses.driverPayouts | currency }}</strong>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span>Payroll Expenses:</span>
                  <span>{{ report()!.expenses.payrollExpenses | currency }}</span>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span>Fuel Costs:</span>
                  <span>{{ report()!.expenses.fuelCosts | currency }}</span>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span>Maintenance:</span>
                  <span>{{ report()!.expenses.maintenanceCosts | currency }}</span>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span>Insurance:</span>
                  <span>{{ report()!.expenses.insuranceCosts | currency }}</span>
                </div>
                <div class="d-flex justify-content-between">
                  <span>Operational:</span>
                  <span>{{ report()!.expenses.operationalExpenses | currency }}</span>
                </div>
                
                <!-- Expenses by Category -->
                <div class="mt-3" *ngIf="report()!.expenses.expensesByCategory.length > 0">
                  <h6>Expenses by Category</h6>
                  <div *ngFor="let category of report()!.expenses.expensesByCategory" class="d-flex justify-content-between mb-1">
                    <span>{{ category.category }}:</span>
                    <span>{{ category.amount | currency }} ({{ category.percentage | number:'1.1-1' }}%)</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Payment Status & Invoice Summary -->
        <div class="row g-4 mb-4">
          <div class="col-xl-6">
            <div class="card">
              <div class="card-header">
                <h5 class="mb-0">Payment Status</h5>
              </div>
              <div class="card-body">
                <div class="row text-center mb-3">
                  <div class="col-4">
                    <h6 class="text-success">{{ report()!.paymentStatus.paidInvoices }}</h6>
                    <small>Paid</small>
                  </div>
                  <div class="col-4">
                    <h6 class="text-warning">{{ report()!.paymentStatus.pendingInvoices }}</h6>
                    <small>Pending</small>
                  </div>
                  <div class="col-4">
                    <h6 class="text-danger">{{ report()!.paymentStatus.overdueInvoices }}</h6>
                    <small>Overdue</small>
                  </div>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span>Total Receivables:</span>
                  <strong>{{ report()!.paymentStatus.totalReceivables | currency }}</strong>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span>Paid Amount:</span>
                  <span class="text-success">{{ report()!.paymentStatus.paidAmount | currency }}</span>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span>Pending Amount:</span>
                  <span class="text-warning">{{ report()!.paymentStatus.pendingAmount | currency }}</span>
                </div>
                <div class="d-flex justify-content-between">
                  <span>Overdue Amount:</span>
                  <span class="text-danger">{{ report()!.paymentStatus.overdueAmount | currency }}</span>
                </div>
              </div>
            </div>
          </div>

          <div class="col-xl-6">
            <div class="card">
              <div class="card-header">
                <h5 class="mb-0">Invoice Summary</h5>
              </div>
              <div class="card-body">
                <div class="d-flex justify-content-between mb-2">
                  <span>Total Invoices:</span>
                  <strong>{{ report()!.invoiceSummary.totalInvoicesIssued }}</strong>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span>Total Value:</span>
                  <strong>{{ report()!.invoiceSummary.totalInvoiceValue | currency }}</strong>
                </div>
                <div class="d-flex justify-content-between mb-2">
                  <span>Average Value:</span>
                  <span>{{ report()!.invoiceSummary.averageInvoiceValue | currency }}</span>
                </div>
                <hr>
                <div class="d-flex justify-content-between text-success mb-1">
                  <span>Paid:</span>
                  <span>{{ report()!.invoiceSummary.invoicesPaid }}</span>
                </div>
                <div class="d-flex justify-content-between text-warning mb-1">
                  <span>Pending:</span>
                  <span>{{ report()!.invoiceSummary.invoicesPending }}</span>
                </div>
                <div class="d-flex justify-content-between text-danger">
                  <span>Overdue:</span>
                  <span>{{ report()!.invoiceSummary.invoicesOverdue }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Top Performers -->
        <div class="row g-4 mb-4" *ngIf="report()!.topDrivers.length > 0 || report()!.topCustomers.length > 0">
          <div class="col-xl-6" *ngIf="report()!.topDrivers.length > 0">
            <div class="card">
              <div class="card-header">
                <h5 class="mb-0">Top Performing Drivers</h5>
              </div>
              <div class="card-body">
                <div *ngFor="let driver of report()!.topDrivers; let i = index" class="d-flex justify-content-between align-items-center mb-2">
                  <div class="d-flex align-items-center">
                    <span class="badge bg-primary me-2">#{{ i + 1 }}</span>
                    <span>{{ driver.name }}</span>
                  </div>
                  <div class="text-end">
                    <div>{{ driver.totalEarnings | currency }}</div>
                    <small class="text-muted">{{ driver.loadsCompleted }} loads</small>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="col-xl-6" *ngIf="report()!.topCustomers.length > 0">
            <div class="card">
              <div class="card-header">
                <h5 class="mb-0">Top Customers</h5>
              </div>
              <div class="card-body">
                <div *ngFor="let customer of report()!.topCustomers; let i = index" class="d-flex justify-content-between align-items-center mb-2">
                  <div class="d-flex align-items-center">
                    <span class="badge bg-success me-2">#{{ i + 1 }}</span>
                    <span>{{ customer.name }}</span>
                  </div>
                  <div class="text-end">
                    <div>{{ customer.totalRevenue | currency }}</div>
                    <small class="text-muted">{{ customer.totalLoads }} loads</small>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Period Comparison -->
        <div class="row g-4" *ngIf="report()!.comparison">
          <div class="col-12">
            <div class="card">
              <div class="card-header">
                <h5 class="mb-0">Period Comparison</h5>
              </div>
              <div class="card-body">
                <div class="row text-center">
                  <div class="col-md-4">
                    <h6>Revenue Growth</h6>
                    <h4 [class]="report()!.comparison!.revenueGrowthPercentage >= 0 ? 'text-success' : 'text-danger'">
                      {{ report()!.comparison!.revenueGrowthPercentage | number:'1.1-1' }}%
                    </h4>
                    <small class="text-muted">{{ report()!.comparison!.revenueGrowth | currency }}</small>
                  </div>
                  <div class="col-md-4">
                    <h6>Profit Growth</h6>
                    <h4 [class]="report()!.comparison!.profitGrowthPercentage >= 0 ? 'text-success' : 'text-danger'">
                      {{ report()!.comparison!.profitGrowthPercentage | number:'1.1-1' }}%
                    </h4>
                    <small class="text-muted">{{ report()!.comparison!.profitGrowth | currency }}</small>
                  </div>
                  <div class="col-md-4">
                    <h6>Load Growth</h6>
                    <h4 [class]="report()!.comparison!.loadGrowthPercentage >= 0 ? 'text-success' : 'text-danger'">
                      {{ report()!.comparison!.loadGrowthPercentage | number:'1.1-1' }}%
                    </h4>
                    <small class="text-muted">{{ report()!.comparison!.loadGrowth }} loads</small>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card {
      box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
    }
    
    .badge {
      font-size: 0.75em;
    }
  `]
})
export class FinancialReportsComponent implements OnInit {
  report = signal<FinancialReportDto | null>(null);
  isLoading = signal<boolean>(false);

  filters: GetFinancialReportQuery = {
    startDate: '',
    endDate: '',
    includeComparison: true,
    includeTopPerformers: true,
    includeInvoiceAging: true,
    topPerformersLimit: 5
  };

  constructor(private reportsApi: ReportsApiService) {}

  ngOnInit(): void {
    this.initializeDateFilters();
  }

  private initializeDateFilters(): void {
    const now = new Date();
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
    const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    
    this.filters.startDate = firstDay.toISOString().split('T')[0];
    this.filters.endDate = lastDay.toISOString().split('T')[0];
  }

  generateReport(): void {
    if (!this.filters.startDate || !this.filters.endDate) {
      return;
    }

    this.isLoading.set(true);
    
    this.reportsApi.getFinancialReport(this.filters).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.success) {
          this.report.set(response.data);
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        console.error('Error loading financial report:', error);
      }
    });
  }

  refreshData(): void {
    this.generateReport();
  }

  exportToPdf(): void {
    this.isLoading.set(true);
    this.reportsApi.exportFinancialReportToPdf(this.filters).subscribe({
      next: (blob) => {
        this.downloadFile(blob, `financial-report-${new Date().toISOString().split('T')[0]}.html`);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error exporting PDF:', error);
        this.isLoading.set(false);
      }
    });
  }

  exportToExcel(): void {
    this.isLoading.set(true);
    this.reportsApi.exportFinancialReportToExcel(this.filters).subscribe({
      next: (blob) => {
        this.downloadFile(blob, `financial-report-${new Date().toISOString().split('T')[0]}.csv`);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error exporting Excel:', error);
        this.isLoading.set(false);
      }
    });
  }

  private downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(url);
  }
}