import {Component, DestroyRef, OnInit, inject, signal} from "@angular/core";
import {takeUntilDestroyed} from "@angular/core/rxjs-interop";
import {FormsModule} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {InputTextModule} from "primeng/inputtext";
import {TableModule} from "primeng/table";
import {ApiService} from "@/core/api";
import {DriverReportDto, DriversReportDto} from "@/core/api/models";
import {ToastService} from "@/core/services";

@Component({
  selector: "app-drivers-report",
  templateUrl: "./drivers-report.html",
  standalone: true,
  imports: [FormsModule, ButtonModule, CardModule, InputTextModule, TableModule],
})
export class DriversReportComponent implements OnInit {
  private readonly apiService = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly toastService = inject(ToastService);
  
  protected readonly items = signal<DriverReportDto[]>([]);
  protected readonly totalCount = signal<number>(0);
  protected readonly isLoading = signal<boolean>(false);
  protected readonly searchQuery = signal<string>("");
  
  ngOnInit(): void {
    this.loadReport();
  }
  
  protected loadReport(): void {
    this.isLoading.set(true);
    
    const query = {
      search: this.searchQuery(),
      pageSize: 100 // We want to show all results for reports
    };

    this.apiService.reportApi
      .getDriversReport(query)
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
          this.toastService.showError("Failed to load the drivers report data");
          this.isLoading.set(false);
        }
      });
  }

  protected reset(): void {
    this.searchQuery.set("");
    this.loadReport();
  }

  protected exportReport(format: string): void {

    this.apiService.reportApi.exportDriversReport(format)
    .subscribe((response) => {
        const blob = response.body!;
        const a = document.createElement("a");
        const url = URL.createObjectURL(blob);
        a.href = url;
        a.download = "loads-report.pdf";
        a.click();
        URL.revokeObjectURL(url);
      });
  }
}

