import {CommonModule} from "@angular/common";
import {Component} from "@angular/core";
import {RouterLink, RouterOutlet} from "@angular/router";

@Component({
  selector: "app-reports-layout",
  template: `
    <h2 class="mb-3">Reports</h2>
    <nav class="flex gap-2 mb-3">
      <a routerLink="loads" routerLinkActive="active">Loads</a>
      <a routerLink="drivers" routerLinkActive="active">Drivers</a>
      <a routerLink="financials" routerLinkActive="active">Financials</a>
    </nav>
    <router-outlet />
  `,
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink],
})
export class ReportsLayoutComponent {}

