import {CommonModule} from "@angular/common";
import {Component, inject, signal} from "@angular/core";
import {HttpClient, HttpParams} from "@angular/common/http";
import {FormsModule} from "@angular/forms";

@Component({
  selector: "app-drivers-report",
  template: `
    <form class="filters" (submit)="$event.preventDefault(); load();">
      <label>From <input type="date" [(ngModel)]="from" name="from"></label>
      <label>To <input type="date" [(ngModel)]="to" name="to"></label>
      <label>Search <input type="text" [(ngModel)]="search" name="search"></label>
      <button type="submit">Filter</button>
      <button type="button" (click)="reset()">Reset</button>
      <button type="button" (click)="export('csv')">Export CSV</button>
      <button type="button" (click)="export('xlsx')">Export Excel</button>
      <button type="button" (click)="export('pdf')">Export PDF</button>
      <button type="button" (click)="export('docx')">Export Word</button>
    </form>
    <table>
      <thead><tr><th>Driver</th><th>Loads</th><th>Distance</th><th>Gross</th></tr></thead>
      <tbody>
      <tr *ngFor="let i of items()"><td>{{i.driverName}}</td><td>{{i.loadsDelivered}}</td><td>{{i.distanceDriven}}</td><td>{{i.grossEarnings}}</td></tr>
      </tbody>
    </table>
    <div class="totals">
      <div>Total Count: {{totalCount()}}</div>
      <div>Total Distance: {{totalDistance()}}</div>
      <div>Total Gross: {{totalGross()}}</div>
    </div>
  `,
  standalone: true,
  imports: [CommonModule, FormsModule],
})
export class DriversReportComponent {
  private readonly http = inject(HttpClient);
  private readonly base = "/reports";
  from?: string;
  to?: string;
  search = "";
  items = signal<any[]>([]);
  totalCount = signal(0);
  totalDistance = signal(0);
  totalGross = signal(0);

  ngOnInit() { this.load(); }
  load() {
    let params = new HttpParams();
    if (this.from) params = params.set('startDate', this.from);
    if (this.to) params = params.set('endDate', this.to);
    if (this.search) params = params.set('search', this.search);
    this.http.get<any>(`${this.base}/drivers`, { params }).subscribe(res => {
      if (res.success) {
        this.items.set(res.data.items); this.totalCount.set(res.data.totalCount);
        this.totalDistance.set(res.data.totalDistance); this.totalGross.set(res.data.totalGross);
      }
    });
  }
  reset() { this.from = this.to = undefined; this.search = ""; this.load(); }
  export(format: string) {
    let params = new HttpParams().set('format', format);
    if (this.from) params = params.set('startDate', this.from);
    if (this.to) params = params.set('endDate', this.to);
    if (this.search) params = params.set('search', this.search);
    this.http.get(`${this.base}/drivers/export`, { params, responseType: 'blob', observe: 'response' }).subscribe(resp => {
      const disp = resp.headers.get('content-disposition') || '';
      const match = /filename="?([^";]+)"?/.exec(disp);
      const filename = match ? match[1] : `drivers-report.${format}`;
      const url = URL.createObjectURL(resp.body!);
      const a = document.createElement('a'); a.href = url; a.download = filename; a.click(); URL.revokeObjectURL(url);
    });
  }
}

