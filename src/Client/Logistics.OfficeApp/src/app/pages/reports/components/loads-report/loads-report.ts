import {CommonModule, DatePipe} from "@angular/common";
import {Component, DestroyRef, OnInit, inject, signal} from "@angular/core";
import {takeUntilDestroyed} from "@angular/core/rxjs-interop";
import {FormsModule} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {InputTextModule} from "primeng/inputtext";
import {TableModule} from "primeng/table";
import {ApiService} from "@/core/api";
import {LoadReportDto, LoadsReportDto} from "@/core/api/models";
import {ToastService} from "@/core/services";
import { ReportApiService } from "@/core/api/services";

@Component({
  selector: "app-loads-report",
  templateUrl: "./loads-report.html",
  styleUrl: "./loads-report.css",
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, CardModule, InputTextModule, TableModule, DatePipe],
})
export class LoadsReportComponent implements OnInit {
  private readonly apiService = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);
  // private readonly reportApi = inject(ReportApiService);
  private readonly toastService = inject(ToastService);
  
  protected readonly items = signal<LoadReportDto[]>([]);
  protected readonly totalCount = signal<number>(0);
  protected readonly totalRevenue = signal<number>(0);
  protected readonly totalDistance = signal<number>(0);
  protected readonly isLoading = signal<boolean>(false);
  
  protected dateFrom = signal<Date | null>(null);
  protected dateTo = signal<Date | null>(null);
  protected searchQuery = signal<string>("");

  ngOnInit(): void {
    this.loadReport();
  }

  protected loadReport(): void {
    this.isLoading.set(true);
    
    const query = {
      dateFrom: this.dateFrom()?.toISOString(),
      dateTo: this.dateTo()?.toISOString(),
      search: this.searchQuery(),
      pageSize: 100 // We want to show all results for reports
    };

    this.apiService.reportApi
      .getLoadsReport(query)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          if (result.success && result.data) {
            this.items.set(result.data.items);
            this.totalCount.set(result.data.totalCount);
          }
          this.isLoading.set(false);
        },
        error: () => {
          this.toastService.showError("Failed to load the report data");
          this.isLoading.set(false);
        }
      });
  }

  protected reset(): void {
    this.dateFrom.set(null);
    this.dateTo.set(null);
    this.searchQuery.set("");
    this.loadReport();
  }

  protected exportReport(format: string): void {

  this.isLoading.set(true);
  
  this.apiService.reportApi.exportLoadsReport({format : "pdf"})
  .subscribe((response) => {
        const blob = response.body!;
        const a = document.createElement("a");
        const url = URL.createObjectURL(blob);
        a.href = url;
        a.download = "loads-report.pdf";
        a.click();
        URL.revokeObjectURL(url);
      });
}}
