import {CommonModule, DatePipe} from "@angular/common";
import {Component, DestroyRef, OnInit, inject, signal} from "@angular/core";
import {takeUntilDestroyed} from "@angular/core/rxjs-interop";
import {FormsModule} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {InputTextModule} from "primeng/inputtext";
import {TableModule} from "primeng/table";
import {ApiService} from "@/core/api";
import {LoadsReportDto} from "@/core/api/models";
import {ToastService} from "@/core/services";

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
  private readonly toastService = inject(ToastService);
  
  protected readonly items = signal<LoadsReportDto[]>([]);
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
            this.items.set(result.data);
            this.totalCount.set(result.totalItems);
            this.calculateTotals(result.data);
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
    // Create a form and submit it to trigger the download
    const form = document.createElement("form");
    form.method = "GET";
    form.action = "/api/reports/loads/export";
    form.target = "_blank";

    // Add form fields for query parameters
    const appendInput = (name: string, value: string) => {
      const input = document.createElement("input");
      input.type = "hidden";
      input.name = name;
      input.value = value;
      form.appendChild(input);
    };

    appendInput("format", format);
    if (this.dateFrom()) {
      appendInput("dateFrom", this.dateFrom()!.toISOString());
    }
    if (this.dateTo()) {
      appendInput("dateTo", this.dateTo()!.toISOString());
    }
    if (this.searchQuery()) {
      appendInput("search", this.searchQuery());
    }

    document.body.appendChild(form);
    form.submit();
    document.body.removeChild(form);
  }

  private calculateTotals(data: LoadsReportDto[]): void {
    const revenue = data.reduce((sum, item) => sum + (item.deliveryCost || 0), 0);
    const distance = data.reduce((sum, item) => sum + (item.distance || 0), 0);

    this.totalRevenue.set(revenue);
    this.totalDistance.set(distance);
  }

}