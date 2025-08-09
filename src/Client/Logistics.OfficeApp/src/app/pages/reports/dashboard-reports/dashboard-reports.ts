import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { 
  ReportsApiService, 
  DashboardReportDto,
  DashboardReportQuery 
} from '../../../core/api/services/reports-api.service';

@Component({
  selector: 'app-dashboard-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container-fluid p-4">
      <!-- Header -->
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 class="mb-1">Dashboard Reports</h2>
          <p class="text-muted mb-0">Executive summary with key metrics across all areas</p>
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

      <!-- Date Range Filter -->
      <div class="card mb-4">
        <div class="card-body">
          <div class="row g-3 align-items-end">
            <div class="col-md-3">
              <label class="form-label">Start Date</label>
              <input type="date" class="form-control" [(ngModel)]="filters.startDate">
            </div>
            <div class="col-md-3">
              <label class="form-label">End Date</label>
              <input type="date" class="form-control" [(ngModel)]="filters.endDate">
            </div>
            <div class="col-md-3">
              <button class="btn btn-primary" (click)="applyFilters()">
                <i class="fas fa-search me-2"></i>Apply Filters
              </button>
            </div>
            <div class="col-md-3 text-end">
              <small class="text-muted">
                Period: {{ formatDateRange() }}
              </small>
            </div>
          </div>
        </div>
      </div>

      <div *ngIf="isLoading()" class="text-center py-5">
        <div class="spinner-border" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
      </div>

      <div *ngIf="!isLoading() && dashboard()">
        <!-- Key Metrics Row -->
        <div class="row g-4 mb-4">
          <!-- Load Metrics -->
          <div class="col-xl-4">
            <div class="card border-primary h-100">
              <div class="card-header bg-primary text-white">
                <h5 class="mb-0"><i class="fas fa-truck me-2"></i>Load Metrics</h5>
              </div>
              <div class="card-body">
                <div class="row g-3">
                  <div class="col-6">
                    <div class="text-center">
                      <h3 class="text-primary mb-1">{{ dashboard()!.loadSummary.totalLoads | number }}</h3>
                      <small class="text-muted">Total Loads</small>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="text-center">
                      <h3 class="text-success mb-1">{{ dashboard()!.loadSummary.completedLoads | number }}</h3>
                      <small class="text-muted">Completed</small>
                    </div>
                  </div>
                  <div class="col-12">
                    <hr>
                    <div class="d-flex justify-content-between">
                      <span>Revenue:</span>
                      <strong class="text-success">{{ dashboard()!.loadSummary.totalRevenue | currency }}</strong>
                    </div>
                    <div class="d-flex justify-content-between">
                      <span>On-Time Delivery:</span>
                      <strong class="text-info">{{ dashboard()!.loadSummary.onTimeDeliveryPercentage | number:'1.1-1' }}%</strong>
                    </div>
                    <div class="d-flex justify-content-between">
                      <span>Avg. Distance:</span>
                      <span>{{ dashboard()!.loadSummary.averageDistance | number:'1.1-1' }} km</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Driver Metrics -->
          <div class="col-xl-4">
            <div class="card border-success h-100">
              <div class="card-header bg-success text-white">
                <h5 class="mb-0"><i class="fas fa-users me-2"></i>Driver Metrics</h5>
              </div>
              <div class="card-body">
                <div class="row g-3">
                  <div class="col-6">
                    <div class="text-center">
                      <h3 class="text-success mb-1">{{ dashboard()!.driverSummary.totalDrivers | number }}</h3>
                      <small class="text-muted">Total Drivers</small>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="text-center">
                      <h3 class="text-primary mb-1">{{ dashboard()!.driverSummary.activeDrivers | number }}</h3>
                      <small class="text-muted">Active</small>
                    </div>
                  </div>
                  <div class="col-12">
                    <hr>
                    <div class="d-flex justify-content-between">
                      <span>Total Earnings:</span>
                      <strong class="text-success">{{ dashboard()!.driverSummary.totalDriverEarnings | currency }}</strong>
                    </div>
                    <div class="d-flex justify-content-between">
                      <span>Avg. Earnings:</span>
                      <span>{{ dashboard()!.driverSummary.averageDriverEarnings | currency }}</span>
                    </div>
                    <div class="d-flex justify-content-between">
                      <span>Distance Driven:</span>
                      <span>{{ dashboard()!.driverSummary.totalDistanceDriven | number:'1.0-0' }} km</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Financial Metrics -->
          <div class="col-xl-4">
            <div class="card border-warning h-100">
              <div class="card-header bg-warning text-dark">
                <h5 class="mb-0"><i class="fas fa-chart-line me-2"></i>Financial Metrics</h5>
              </div>
              <div class="card-body">
                <div class="row g-3">
                  <div class="col-6">
                    <div class="text-center">
                      <h3 class="text-success mb-1">{{ dashboard()!.financialSummary.totalRevenue | currency:'USD':'symbol':'1.0-0' }}</h3>
                      <small class="text-muted">Revenue</small>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="text-center">
                      <h3 class="text-danger mb-1">{{ dashboard()!.financialSummary.totalExpenses | currency:'USD':'symbol':'1.0-0' }}</h3>
                      <small class="text-muted">Expenses</small>
                    </div>
                  </div>
                  <div class="col-12">
                    <hr>
                    <div class="d-flex justify-content-between">
                      <span>Net Profit:</span>
                      <strong [class]="dashboard()!.financialSummary.netProfit >= 0 ? 'text-success' : 'text-danger'">
                        {{ dashboard()!.financialSummary.netProfit | currency }}
                      </strong>
                    </div>
                    <div class="d-flex justify-content-between">
                      <span>Profit Margin:</span>
                      <strong [class]="dashboard()!.financialSummary.profitMargin >= 0 ? 'text-success' : 'text-danger'">
                        {{ dashboard()!.financialSummary.profitMargin | number:'1.1-1' }}%
                      </strong>
                    </div>
                    <div class="d-flex justify-content-between">
                      <span>Collection Rate:</span>
                      <span class="text-info">{{ dashboard()!.financialSummary.collectionRate | number:'1.1-1' }}%</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Charts and Detailed Analysis -->
        <div class="row g-4">
          <!-- Load Status Distribution -->
          <div class="col-xl-6">
            <div class="card">
              <div class="card-header">
                <h5 class="mb-0">Load Status Distribution</h5>
              </div>
              <div class="card-body">
                <div *ngFor="let status of dashboard()!.loadSummary.loadsByStatus" class="d-flex justify-content-between align-items-center mb-2">
                  <div class="d-flex align-items-center">
                    <span class="badge me-2" [class]="getStatusBadgeClass(status.status)">{{ status.status }}</span>
                    <span>{{ status.count }} loads</span>
                  </div>
                  <div class="text-end">
                    <div>{{ status.revenue | currency }}</div>
                    <small class="text-muted">{{ status.percentage | number:'1.1-1' }}%</small>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Load Type Distribution -->
          <div class="col-xl-6">
            <div class="card">
              <div class="card-header">
                <h5 class="mb-0">Load Type Distribution</h5>
              </div>
              <div class="card-body">
                <div *ngFor="let type of dashboard()!.loadSummary.loadsByType" class="d-flex justify-content-between align-items-center mb-2">
                  <div class="d-flex align-items-center">
                    <span class="badge bg-secondary me-2">{{ type.type }}</span>
                    <span>{{ type.count }} loads</span>
                  </div>
                  <div class="text-end">
                    <div>{{ type.revenue | currency }}</div>
                    <small class="text-muted">{{ type.percentage | number:'1.1-1' }}%</small>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Financial Trends -->
          <div class="col-12" *ngIf="dashboard()!.financialSummary.monthlyTrends && dashboard()!.financialSummary.monthlyTrends.length > 0">
            <div class="card">
              <div class="card-header">
                <h5 class="mb-0">Monthly Financial Trends</h5>
              </div>
              <div class="card-body">
                <div class="table-responsive">
                  <table class="table table-sm">
                    <thead>
                      <tr>
                        <th>Month</th>
                        <th>Revenue</th>
                        <th>Expenses</th>
                        <th>Profit</th>
                        <th>Loads</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr *ngFor="let trend of dashboard()!.financialSummary.monthlyTrends">
                        <td>{{ trend.month }}</td>
                        <td>{{ trend.revenue | currency }}</td>
                        <td>{{ trend.expenses | currency }}</td>
                        <td [class]="trend.profit >= 0 ? 'text-success' : 'text-danger'">
                          {{ trend.profit | currency }}
                        </td>
                        <td>{{ trend.loadsCompleted | number }}</td>
                      </tr>
                    </tbody>
                  </table>
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
      border: 1px solid rgba(0, 0, 0, 0.125);
    }
    
    .card-header {
      border-bottom: 1px solid rgba(0, 0, 0, 0.125);
    }
    
    .badge {
      font-size: 0.75em;
    }
  `]
})
export class DashboardReportsComponent implements OnInit {
  dashboard = signal<DashboardReportDto | null>(null);
  isLoading = signal<boolean>(false);

  filters: DashboardReportQuery = {};

  constructor(
    private reportsApi: ReportsApiService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Check for query parameters from route
    this.route.queryParams.subscribe(params => {
      if (params['startDate']) this.filters.startDate = params['startDate'];
      if (params['endDate']) this.filters.endDate = params['endDate'];
    });

    this.initializeDateFilters();
    this.loadData();
  }

  private initializeDateFilters(): void {
    if (!this.filters.startDate) {
      const now = new Date();
      const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
      this.filters.startDate = firstDay.toISOString().split('T')[0];
    }
    
    if (!this.filters.endDate) {
      const now = new Date();
      const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0);
      this.filters.endDate = lastDay.toISOString().split('T')[0];
    }
  }

  loadData(): void {
    this.isLoading.set(true);
    
    this.reportsApi.getDashboardReport(this.filters).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.success) {
          this.dashboard.set(response.data);
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        console.error('Error loading dashboard data:', error);
      }
    });
  }

  applyFilters(): void {
    this.loadData();
  }

  refreshData(): void {
    this.loadData();
  }

  formatDateRange(): string {
    if (this.filters.startDate && this.filters.endDate) {
      return `${this.filters.startDate} to ${this.filters.endDate}`;
    }
    return 'Current Month';
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Delivered': return 'bg-success';
      case 'PickedUp': return 'bg-warning';
      case 'Dispatched': return 'bg-primary';
      default: return 'bg-secondary';
    }
  }

  exportToPdf(): void {
    this.isLoading.set(true);
    this.reportsApi.exportDashboardReportToPdf(this.filters).subscribe({
      next: (blob) => {
        this.downloadFile(blob, `dashboard-report-${new Date().toISOString().split('T')[0]}.html`);
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
    this.reportsApi.exportDashboardReportToExcel(this.filters).subscribe({
      next: (blob) => {
        this.downloadFile(blob, `dashboard-report-${new Date().toISOString().split('T')[0]}.csv`);
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