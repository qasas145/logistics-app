import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { 
  ReportsApiService, 
  LoadReportDto, 
  LoadReportSummaryDto, 
  GetLoadReportQuery 
} from '../../../core/api/services/reports-api.service';

@Component({
  selector: 'app-load-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="container-fluid p-4">
      <!-- Header -->
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 class="mb-1">Load Reports</h2>
          <p class="text-muted mb-0">Comprehensive analysis of loads, delivery performance, and revenue</p>
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

      <!-- Filters Card -->
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
                <option value="Dispatched">Dispatched</option>
                <option value="PickedUp">Picked Up</option>
                <option value="Delivered">Delivered</option>
              </select>
            </div>
            <div class="col-md-3">
              <label class="form-label">Load Type</label>
              <select class="form-control" [(ngModel)]="filters.loadType">
                <option value="">All Types</option>
                <option value="General">General</option>
                <option value="Refrigerated">Refrigerated</option>
                <option value="Hazmat">Hazmat</option>
                <option value="Oversized">Oversized</option>
              </select>
            </div>
            <div class="col-md-3">
              <label class="form-label">Min Cost</label>
              <input type="number" class="form-control" [(ngModel)]="filters.minDeliveryCost" placeholder="Minimum cost">
            </div>
            <div class="col-md-3">
              <label class="form-label">Max Cost</label>
              <input type="number" class="form-control" [(ngModel)]="filters.maxDeliveryCost" placeholder="Maximum cost">
            </div>
            <div class="col-md-6">
              <label class="form-label">&nbsp;</label>
              <div class="d-flex gap-2">
                <button class="btn btn-primary" (click)="applyFilters()">
                  <i class="fas fa-search me-2"></i>Apply Filters
                </button>
                <button class="btn btn-outline-secondary" (click)="clearFilters()">
                  <i class="fas fa-times me-2"></i>Clear
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Summary Cards -->
      <div class="row g-4 mb-4" *ngIf="summary()">
        <div class="col-xl-3 col-md-6">
          <div class="card border-primary">
            <div class="card-body text-center">
              <i class="fas fa-truck fa-2x text-primary mb-3"></i>
              <h4 class="mb-1">{{ summary()!.totalLoads | number }}</h4>
              <p class="text-muted mb-0">Total Loads</p>
            </div>
          </div>
        </div>
        <div class="col-xl-3 col-md-6">
          <div class="card border-success">
            <div class="card-body text-center">
              <i class="fas fa-check-circle fa-2x text-success mb-3"></i>
              <h4 class="mb-1">{{ summary()!.completedLoads | number }}</h4>
              <p class="text-muted mb-0">Completed</p>
            </div>
          </div>
        </div>
        <div class="col-xl-3 col-md-6">
          <div class="card border-info">
            <div class="card-body text-center">
              <i class="fas fa-dollar-sign fa-2x text-info mb-3"></i>
              <h4 class="mb-1">{{ summary()!.totalRevenue | currency }}</h4>
              <p class="text-muted mb-0">Total Revenue</p>
            </div>
          </div>
        </div>
        <div class="col-xl-3 col-md-6">
          <div class="card border-warning">
            <div class="card-body text-center">
              <i class="fas fa-percentage fa-2x text-warning mb-3"></i>
              <h4 class="mb-1">{{ summary()!.onTimeDeliveryPercentage | number:'1.1-1' }}%</h4>
              <p class="text-muted mb-0">On-Time Delivery</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Loads Table -->
      <div class="card">
        <div class="card-header d-flex justify-content-between align-items-center">
          <h5 class="mb-0">Load Details</h5>
          <div class="d-flex align-items-center gap-3">
            <span class="text-muted">{{ totalRecords() }} records</span>
            <div class="d-flex align-items-center gap-2">
              <label class="form-label mb-0">Page Size:</label>
              <select class="form-control form-control-sm" style="width: 80px;" [(ngModel)]="pageSize" (change)="applyFilters()">
                <option value="10">10</option>
                <option value="25">25</option>
                <option value="50">50</option>
                <option value="100">100</option>
              </select>
            </div>
          </div>
        </div>
        <div class="card-body">
          <div class="table-responsive">
            <table class="table table-hover">
              <thead class="table-light">
                <tr>
                  <th style="cursor: pointer;" (click)="sort('number')">
                    Load # <i class="fas fa-sort"></i>
                  </th>
                  <th style="cursor: pointer;" (click)="sort('name')">
                    Name <i class="fas fa-sort"></i>
                  </th>
                  <th>Origin → Destination</th>
                  <th style="cursor: pointer;" (click)="sort('status')">
                    Status <i class="fas fa-sort"></i>
                  </th>
                  <th style="cursor: pointer;" (click)="sort('deliveryCost')">
                    Cost <i class="fas fa-sort"></i>
                  </th>
                  <th>Driver</th>
                  <th style="cursor: pointer;" (click)="sort('dispatchedDate')">
                    Dispatched <i class="fas fa-sort"></i>
                  </th>
                  <th>Payment</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let load of loads(); trackBy: trackByLoadId">
                  <td><strong>#{{ load.number }}</strong></td>
                  <td>{{ load.name }}</td>
                  <td>
                    <small class="text-muted">
                      {{ load.originAddress }}<br>
                      → {{ load.destinationAddress }}
                    </small>
                  </td>
                  <td>
                    <span class="badge" [class]="getStatusBadgeClass(load.status)">
                      {{ load.status }}
                    </span>
                  </td>
                  <td>
                    <strong>{{ load.deliveryCost | currency }}</strong><br>
                    <small class="text-muted">{{ load.distance | number:'1.1-1' }} km</small>
                  </td>
                  <td>
                    <span *ngIf="load.assignedDriverName">{{ load.assignedDriverName }}</span>
                    <span *ngIf="!load.assignedDriverName" class="text-muted">Unassigned</span><br>
                    <small class="text-muted" *ngIf="load.assignedTruckNumber">Truck: {{ load.assignedTruckNumber }}</small>
                  </td>
                  <td>
                    {{ load.dispatchedDate | date:'short' }}<br>
                    <small class="text-muted" *ngIf="load.deliveryDate">
                      Delivered: {{ load.deliveryDate | date:'short' }}
                    </small>
                  </td>
                  <td>
                    <span *ngIf="load.hasInvoice">
                      <span class="badge" [class]="getPaymentBadgeClass(load.isPaid)">
                        {{ load.isPaid ? 'Paid' : 'Pending' }}
                      </span><br>
                      <small class="text-muted">{{ load.invoiceTotal | currency }}</small>
                    </span>
                    <span *ngIf="!load.hasInvoice" class="text-muted">No Invoice</span>
                  </td>
                  <td>
                    <div class="btn-group btn-group-sm">
                      <button class="btn btn-outline-primary btn-sm" (click)="viewLoadDetails(load)">
                        <i class="fas fa-eye"></i>
                      </button>
                      <button class="btn btn-outline-success btn-sm" (click)="exportSingleLoad(load)">
                        <i class="fas fa-download"></i>
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
    .table th {
      border-top: none;
      font-weight: 600;
    }
    
    .badge {
      font-size: 0.75em;
    }
    
    .btn-group-sm .btn {
      padding: 0.25rem 0.5rem;
    }
  `]
})
export class LoadReportsComponent implements OnInit {
  // Signals for reactive data
  loads = signal<LoadReportDto[]>([]);
  summary = signal<LoadReportSummaryDto | null>(null);
  totalRecords = signal<number>(0);
  currentPage = signal<number>(1);
  totalPages = signal<number>(0);
  isLoading = signal<boolean>(false);

  // Component state
  pageSize = 25;
  sortBy = 'dispatchedDate';
  sortOrder = 'desc';

  // Filters
  filters: GetLoadReportQuery = {
    page: 1,
    pageSize: 25,
    sortBy: 'dispatchedDate',
    sortOrder: 'desc'
  };

  constructor(private reportsApi: ReportsApiService) {}

  ngOnInit(): void {
    this.initializeDateFilters();
    this.loadData();
  }

  private initializeDateFilters(): void {
    // Default to current month
    const now = new Date();
    const firstDay = new Date(now.getFullYear(), now.getMonth(), 1);
    const lastDay = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    
    this.filters.startDate = firstDay.toISOString().split('T')[0];
    this.filters.endDate = lastDay.toISOString().split('T')[0];
  }

  loadData(): void {
    this.isLoading.set(true);
    this.loadSummary();
    this.loadLoads();
  }

  private loadLoads(): void {
    this.filters.page = this.currentPage();
    this.filters.pageSize = this.pageSize;
    this.filters.sortBy = this.sortBy;
    this.filters.sortOrder = this.sortOrder;

    this.reportsApi.getLoadReports(this.filters).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        if (response.success) {
          this.loads.set(response.data.items || []);
          this.totalRecords.set(response.data.totalCount || 0);
          this.totalPages.set(response.data.totalPages || 0);
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        console.error('Error loading loads:', error);
      }
    });
  }

  private loadSummary(): void {
    const summaryQuery = {
      startDate: this.filters.startDate,
      endDate: this.filters.endDate,
      status: this.filters.status,
      loadType: this.filters.loadType
    };

    this.reportsApi.getLoadReportSummary(summaryQuery).subscribe({
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

  clearFilters(): void {
    this.filters = {
      page: 1,
      pageSize: this.pageSize,
      sortBy: 'dispatchedDate',
      sortOrder: 'desc'
    };
    this.initializeDateFilters();
    this.loadData();
  }

  refreshData(): void {
    this.loadData();
  }

  sort(field: string): void {
    if (this.sortBy === field) {
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = field;
      this.sortOrder = 'asc';
    }
    this.applyFilters();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      this.loadLoads();
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

  trackByLoadId(index: number, load: LoadReportDto): string {
    return load.id;
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Delivered': return 'badge-success';
      case 'PickedUp': return 'badge-warning';
      case 'Dispatched': return 'badge-primary';
      default: return 'badge-secondary';
    }
  }

  getPaymentBadgeClass(isPaid: boolean): string {
    return isPaid ? 'badge-success' : 'badge-warning';
  }

  viewLoadDetails(load: LoadReportDto): void {
    // TODO: Implement load details modal or navigation
    console.log('View load details:', load);
  }

  exportSingleLoad(load: LoadReportDto): void {
    // TODO: Implement single load export
    console.log('Export single load:', load);
  }

  exportToPdf(): void {
    this.isLoading.set(true);
    this.reportsApi.exportLoadReportToPdf(this.filters).subscribe({
      next: (blob) => {
        this.downloadFile(blob, `load-report-${new Date().toISOString().split('T')[0]}.pdf`);
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
    this.reportsApi.exportLoadReportToExcel(this.filters).subscribe({
      next: (blob) => {
        this.downloadFile(blob, `load-report-${new Date().toISOString().split('T')[0]}.xlsx`);
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