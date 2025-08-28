import {CommonModule} from "@angular/common";
import {Component, inject, signal} from "@angular/core";
import {HttpClient, HttpParams} from "@angular/common/http";
import {FormsModule} from "@angular/forms";

@Component({
  selector: "app-loads-report",
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
      <thead>
      <tr>
        <th>#</th><th>Name</th><th>Status</th><th>Created</th><th>Delivered</th><th>Truck</th><th>Customer</th><th>Revenue</th><th>Distance</th>
      </tr>
      </thead>
      <tbody>
      <tr *ngFor="let i of items()">
        <td>{{i.number}}</td><td>{{i.name}}</td><td>{{i.status}}</td><td>{{i.createdAt | date:'yyyy-MM-dd'}}</td>
        <td>{{i.deliveredAt | date:'yyyy-MM-dd'}}</td><td>{{i.truckNumber}}</td><td>{{i.customerName}}</td><td>{{i.deliveryCost}}</td><td>{{i.distance}}</td>
      </tr>
      </tbody>
    </table>
    <div class="totals">
      <div>Total Count: {{totalCount()}}</div>
      <div>Total Revenue: {{totalRevenue()}}</div>
      <div>Total Distance: {{totalDistance()}}</div>
    </div>
  `,
  standalone: true,
  imports: [CommonModule, FormsModule],
})
export class LoadsReportComponent {
  private readonly http = inject(HttpClient);
  private readonly base = "/reports";
  from?: string;
  to?: string;
  search = "";
  items = signal<any[]>([]);
  totalCount = signal(0);
  totalRevenue = signal(0);
  totalDistance = signal(0);

  ngOnInit() { this.load(); }

  load() {
    let params = new HttpParams();
    if (this.from) params = params.set('startDate', this.from);
    if (this.to) params = params.set('endDate', this.to);
    if (this.search) params = params.set('search', this.search);
    this.http.get<any>(`${this.base}/loads`, { params }).subscribe(res => {
      if (res.success) {
        this.items.set(res.data.items);
        this.totalCount.set(res.data.totalCount);
        this.totalRevenue.set(res.data.totalRevenue);
        this.totalDistance.set(res.data.totalDistance);
      }
    });
  }

  reset() {
    this.from = this.to = undefined;
    this.search = "";
    this.load();
  }

  export(format: string) {
    let params = new HttpParams().set('format', format);
    if (this.from) params = params.set('startDate', this.from);
    if (this.to) params = params.set('endDate', this.to);
    if (this.search) params = params.set('search', this.search);
    this.http.get(`${this.base}/loads/export`, { params, responseType: 'blob', observe: 'response' }).subscribe(resp => {
      const disp = resp.headers.get('content-disposition') || '';
      const match = /filename="?([^";]+)"?/.exec(disp);
      const filename = match ? match[1] : `loads-report.${format}`;
      const url = URL.createObjectURL(resp.body!);
      const a = document.createElement('a'); a.href = url; a.download = filename; a.click(); URL.revokeObjectURL(url);
    });
  }
}

