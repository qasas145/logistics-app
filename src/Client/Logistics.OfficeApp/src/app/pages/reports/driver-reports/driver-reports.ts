import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { 
  ReportsApiService, 
  DriverReportDto, 
  DriverReportSummaryDto, 
  GetDriverReportQuery 
} from '../../../core/api/services/reports-api.service';

@Component({
  selector: 'app-driver-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="container-fluid p-4">
      <!-- Header -->
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 class="mb-1">Driver Reports</h2>
          <p class="text-muted mb-0">Driver performance, earnings, and efficiency metrics</p>
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
          <h5 class="mb-0">Filters</h5>
        </div>
        <div class="card-body">
          <div class="row g-3">
            <div class="col-md-3">
              <label class="form-label">Start Date</label>
              <input type="date" class="form-control" [(ngModel)]="filters.startDate">
            </div>
            <div class="col-md-3">
              <label class="form-label">End Date</label>
              <input type="date" class="form-control" [(ngModel)]="filters.endDate">
            </div>
            <div class="col-md-3">
              <label class="form-label">Status</label>
              <select class="form-control" [(ngModel)]="filters.status">
                <option value="">All Statuses</option>
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
              </select>
            </div>
            <div class="col-md-3">
              <button class="btn btn-primary" (click)="applyFilters()">
                <i class="fas fa-search me-2"></i>Apply Filters
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Summary Cards -->
      <div class="row g-4 mb-4" *ngIf="summary()">
        <div class="col-xl-3 col-md-6">
          <div class="card border-primary">
            <div class="card-body text-center">
              <i class="fas fa-users fa-2x text-primary mb-3"></i>
              <h4 class="mb-1">{{ summary()!.totalDrivers | number }}</h4>
              <p class="text-muted mb-0">Total Drivers</p>
            </div>
          </div>
        </div>
        <div class="col-xl-3 col-md-6">
          <div class="card border-success">
            <div class="card-body text-center">
              <i class="fas fa-user-check fa-2x text-success mb-3"></i>
              <h4 class="mb-1">{{ summary()!.activeDrivers | number }}</h4>
              <p class="text-muted mb-0">Active Drivers</p>
            </div>
          </div>
        </div>
        <div class="col-xl-3 col-md-6">
          <div class="card border-info">
            <div class="card-body text-center">
              <i class="fas fa-dollar-sign fa-2x text-info mb-3"></i>
              <h4 class="mb-1">{{ summary()!.totalDriverEarnings | currency }}</h4>
              <p class="text-muted mb-0">Total Earnings</p>
            </div>
          </div>
        </div>
        <div class="col-xl-3 col-md-6">
          <div class="card border-warning">
            <div class="card-body text-center">
              <i class="fas fa-percentage fa-2x text-warning mb-3"></i>
              <h4 class="mb-1">{{ summary()!.overallOnTimePercentage | number:'1.1-1' }}%</h4>
              <p class="text-muted mb-0">On-Time Delivery</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Drivers Table -->
      <div class="card">
        <div class="card-header d-flex justify-content-between align-items-center">
          <h5 class="mb-0">Driver Performance</h5>
          <div class="d-flex align-items-center gap-3">
            <span class="text-muted">{{ totalRecords() }} drivers</span>
            <div class="d-flex align-items-center gap-2">
              <label class="form-label mb-0">Page Size:</label>
              <select class="form-control form-control-sm" style="width: 80px;" [(ngModel)]="pageSize" (change)="applyFilters()">
                <option value="10">10</option>
                <option value="25">25</option>
                <option value="50">50</option>
              </select>
            </div>
          </div>
        </div>
        <div class="card-body">
          <div class="table-responsive">
            <table class="table table-hover">
              <thead class="table-light">
                <tr>
                  <th>Driver</th>
                  <th>Truck</th>
                  <th>Performance</th>
                  <th>Earnings</th>
                  <th>Distance</th>
                  <th>On-Time %</th>
                  <th>This Month</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let driver of drivers(); trackBy: trackByDriverId">
                  <td>
                    <div>
                      <strong>{{ driver.fullName }}</strong><br>
                      <small class="text-muted">{{ driver.email }}</small>
                    </div>
                  </td>
                  <td>
                    <span *ngIf="driver.currentTruckNumber">{{ driver.currentTruckNumber }}</span>
                    <span *ngIf="!driver.currentTruckNumber" class="text-muted">Unassigned</span><br>
                    <small class="text-muted" *ngIf="driver.truckModel">{{ driver.truckModel }}</small>
                  </td>
                  <td>
                    <div class="small">
                      <div>Completed: <strong>{{ driver.totalLoadsCompleted }}</strong></div>
                      <div>In Progress: {{ driver.totalLoadsInProgress }}</div>
                      <div>Dispatched: {{ driver.totalLoadsDispatched }}</div>
                    </div>
                  </td>
                  <td>
                    <div>
                      <strong>{{ driver.totalEarnings | currency }}</strong><br>
                      <small class="text-muted">Avg: {{ driver.averageEarningsPerLoad | currency }}</small>
                    </div>
                  </td>
                  <td>
                    <div>
                      <strong>{{ driver.totalDistanceDriven | number:'1.0-0' }} km</strong><br>
                      <small class="text-muted">Avg: {{ driver.averageDistancePerLoad | number:'1.1-1' }} km</small>
                    </div>
                  </td>
                  <td>
                    <div class="text-center">
                      <div class="progress mb-1" style="height: 8px;">
                        <div class="progress-bar" 
                             [class]="getOnTimeProgressClass(driver.onTimeDeliveryPercentage)"
                             [style.width.%]="driver.onTimeDeliveryPercentage">
                        </div>
                      </div>
                      <small>{{ driver.onTimeDeliveryPercentage | number:'1.1-1' }}%</small>
                    </div>
                  </td>
                  <td>
                    <div class="small">
                      <div>Loads: {{ driver.thisMonth.loadsCompleted }}</div>
                      <div>Earnings: {{ driver.thisMonth.totalEarnings | currency }}</div>
                      <div>On-Time: {{ driver.thisMonth.onTimePercentage | number:'1.1-1' }}%</div>
                    </div>
                  </td>
                  <td>
                    <div class="btn-group btn-group-sm">
                      <button class="btn btn-outline-primary btn-sm" (click)="viewDriverDetails(driver)">
                        <i class="fas fa-eye"></i>
                      </button>
                      <button class="btn btn-outline-info btn-sm" (click)="viewDriverLoads(driver)">
                        <i class="fas fa-truck"></i>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Pagination -->
          <nav *ngIf="totalPages() > 1">
            <ul class="pagination justify-content-center">
              <li class="page-item" [class.disabled]="currentPage() === 1">
                <button class="page-link" (click)="goToPage(currentPage() - 1)">Previous</button>
              </li>
              <li class="page-item" 
                  *ngFor="let page of getPageNumbers()" 
                  [class.active]="page === currentPage()">
                <button class="page-link" (click)="goToPage(page)">{{ page }}</button>
              </li>
              <li class="page-item" [class.disabled]="currentPage() === totalPages()">
                <button class="page-link" (click)="goToPage(currentPage() + 1)">Next</button>
              </li>
            </ul>
          </nav>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .progress {
      background-color: #e9ecef;
    }
    
    .table th {
      border-top: none;
      font-weight: 600;
    }
    
    .btn-group-sm .btn {
      padding: 0.25rem 0.5rem;
    }
  `]
})
export class DriverReportsComponent implements OnInit {
  drivers = signal<DriverReportDto[]>([]);
  summary = signal<DriverReportSummaryDto | null>(null);
  totalRecords = signal<number>(0);
  currentPage = signal<number>(1);
  totalPages = signal<number>(0);
  isLoading = signal<boolean>(false);

  pageSize = 25;
  sortBy = 'totalEarnings';
  sortOrder = 'desc';

  filters: GetDriverReportQuery = {
    page: 1,
    pageSize: 25,
    sortBy: 'totalEarnings',
    sortOrder: 'desc',
    includeRecentLoads: false
  };

  constructor(private reportsApi: ReportsApiService) {}

  ngOnInit(): void {
    this.initializeDateFilters();
    this.loadData();
  }

  private initializeDateFilters(): void {
    const now = new Date();
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
    const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    
    this.filters.startDate = firstDay.toISOString().split('T')[0];
    this.filters.endDate = lastDay.toISOString().split('T')[0];
  }

  loadData(): void {
    this.isLoading.set(true);
    this.loadSummary();
    this.loadDrivers();
  }

  private loadDrivers(): void {
    this.filters.page = this.currentPage();
    this.filters.pageSize = this.pageSize;
    this.filters.sortBy = this.sortBy;
    this.filters.sortOrder = this.sortOrder;

    this.reportsApi.getDriverReports(this.filters).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.success) {
          this.drivers.set(response.data.items || []);
          this.totalRecords.set(response.data.totalCount || 0);
          this.totalPages.set(response.data.totalPages || 0);
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        console.error('Error loading drivers:', error);
      }
    });
  }

  private loadSummary(): void {
    const summaryQuery = {
      startDate: this.filters.startDate,
      endDate: this.filters.endDate,
      status: this.filters.status
    };

    this.reportsApi.getDriverReportSummary(summaryQuery).subscribe({
      next: (response) => {
        if (response.success) {
          this.summary.set(response.data);
        }
      },
      error: (error) => {
        console.error('Error loading summary:', error);
      }
    });
  }

  applyFilters(): void {
    this.currentPage.set(1);
    this.loadData();
  }

  refreshData(): void {
    this.loadData();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      this.loadDrivers();
    }
  }

  getPageNumbers(): number[] {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: number[] = [];
    
    let start = Math.max(1, current - 2);
    let end = Math.min(total, current + 2);
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  trackByDriverId(index: number, driver: DriverReportDto): string {
    return driver.id;
  }

  getOnTimeProgressClass(percentage: number): string {
    if (percentage >= 90) return 'bg-success';
    if (percentage >= 75) return 'bg-warning';
    return 'bg-danger';
  }

  viewDriverDetails(driver: DriverReportDto): void {
    // TODO: Implement driver details modal or navigation
    console.log('View driver details:', driver);
  }

  viewDriverLoads(driver: DriverReportDto): void {
    // TODO: Navigate to loads filtered by this driver
    console.log('View driver loads:', driver);
  }

  exportToPdf(): void {
    this.isLoading.set(true);
    this.reportsApi.exportDriverReportToPdf(this.filters).subscribe({
      next: (blob) => {
        this.downloadFile(blob, `driver-report-${new Date().toISOString().split('T')[0]}.html`);
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
    this.reportsApi.exportDriverReportToExcel(this.filters).subscribe({
      next: (blob) => {
        this.downloadFile(blob, `driver-report-${new Date().toISOString().split('T')[0]}.csv`);
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