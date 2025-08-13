import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

interface ReportCard {
  title: string;
  description: string;
  icon: string;
  route: string;
  color: string;
}

@Component({
  selector: 'app-reports-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid p-4">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="mb-0">Reports Dashboard</h2>
        <p class="text-muted mb-0">Generate and view comprehensive business reports</p>
      </div>

      <div class="row g-4">
        <div class="col-xl-3 col-lg-4 col-md-6" *ngFor="let report of reportCards">
          <div class="card h-100 report-card" 
               [class]="'border-' + report.color"
               (click)="navigateToReport(report.route)"
               style="cursor: pointer;">
            <div class="card-body text-center">
              <div class="mb-3">
                <i [class]="report.icon + ' fa-3x text-' + report.color"></i>
              </div>
              <h5 class="card-title">{{ report.title }}</h5>
              <p class="card-text text-muted">{{ report.description }}</p>
              <div class="mt-auto">
                <button class="btn btn-outline-{{ report.color }} btn-sm">
                  View Report <i class="fas fa-arrow-right ms-1"></i>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="row mt-5">
        <div class="col-12">
          <div class="card">
            <div class="card-header">
              <h5 class="mb-0">Quick Actions</h5>
            </div>
            <div class="card-body">
              <div class="row g-3">
                <div class="col-auto">
                  <button class="btn btn-primary" (click)="navigateToReport('dashboard')">
                    <i class="fas fa-tachometer-alt me-2"></i>Dashboard Overview
                  </button>
                </div>
                <div class="col-auto">
                  <button class="btn btn-outline-primary" (click)="generateMonthlyReport()">
                    <i class="fas fa-calendar-alt me-2"></i>Monthly Report
                  </button>
                </div>
                <div class="col-auto">
                  <button class="btn btn-outline-success" (click)="generateQuarterlyReport()">
                    <i class="fas fa-chart-bar me-2"></i>Quarterly Report
                  </button>
                </div>
                <div class="col-auto">
                  <button class="btn btn-outline-info" (click)="scheduleReport()">
                    <i class="fas fa-clock me-2"></i>Schedule Report
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .report-card {
      transition: all 0.3s ease;
      border-width: 2px !important;
    }
    
    .report-card:hover {
      transform: translateY(-5px);
      box-shadow: 0 8px 25px rgba(0,0,0,0.15);
    }
    
    .card-body {
      display: flex;
      flex-direction: column;
      height: 100%;
    }
    
    .mt-auto {
      margin-top: auto !important;
    }
  `]
})
export class ReportsListComponent {

  reportCards: ReportCard[] = [
    {
      title: 'Dashboard Reports',
      description: 'Executive summary with key metrics across all areas',
      icon: 'fas fa-tachometer-alt',
      route: 'dashboard',
      color: 'primary'
    },
    {
      title: 'Load Reports',
      description: 'Detailed analysis of loads, delivery performance, and revenue',
      icon: 'fas fa-truck',
      route: 'loads',
      color: 'info'
    },
    {
      title: 'Driver Reports',
      description: 'Driver performance, earnings, and efficiency metrics',
      icon: 'fas fa-users',
      route: 'drivers',
      color: 'success'
    },
    {
      title: 'Financial Reports',
      description: 'Revenue analysis, cash flow, and profitability insights',
      icon: 'fas fa-chart-line',
      route: 'financial',
      color: 'warning'
    }
  ];

  constructor(private router: Router) {}

  navigateToReport(route: string): void {
    this.router.navigate(['/reports', route]);
  }

  generateMonthlyReport(): void {
    // Navigate to dashboard with monthly filter
    this.router.navigate(['/reports/dashboard'], {
      queryParams: {
        period: 'monthly',
        startDate: this.getFirstDayOfMonth(),
        endDate: this.getLastDayOfMonth()
      }
    });
  }

  generateQuarterlyReport(): void {
    // Navigate to dashboard with quarterly filter
    this.router.navigate(['/reports/dashboard'], {
      queryParams: {
        period: 'quarterly',
        startDate: this.getFirstDayOfQuarter(),
        endDate: this.getLastDayOfQuarter()
      }
    });
  }

  scheduleReport(): void {
    // TODO: Implement report scheduling functionality
    alert('Report scheduling feature coming soon!');
  }

  private getFirstDayOfMonth(): string {
    const date = new Date();
    return new Date(date.getFullYear(), date.getMonth(), 1).toISOString().split('T')[0];
  }

  private getLastDayOfMonth(): string {
    const date = new Date();
    return new Date(date.getFullYear(), date.getMonth() + 1, 0).toISOString().split('T')[0];
  }

  private getFirstDayOfQuarter(): string {
    const date = new Date();
    const quarter = Math.floor(date.getMonth() / 3);
    return new Date(date.getFullYear(), quarter * 3, 1).toISOString().split('T')[0];
  }

  private getLastDayOfQuarter(): string {
    const date = new Date();
    const quarter = Math.floor(date.getMonth() / 3);
    return new Date(date.getFullYear(), quarter * 3 + 3, 0).toISOString().split('T')[0];
  }
}