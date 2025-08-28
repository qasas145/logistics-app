import {Routes} from "@angular/router";

export const reportsRoutes: Routes = [
  {
    path: "",
    loadComponent: () => import("./reports.layout").then(m => m.ReportsLayoutComponent),
    children: [
      { path: "loads", loadComponent: () => import("./views/loads").then(m => m.LoadsReportComponent) },
      { path: "drivers", loadComponent: () => import("./views/drivers").then(m => m.DriversReportComponent) },
      { path: "financials", loadComponent: () => import("./views/financials").then(m => m.FinancialsReportComponent) },
      { path: "", redirectTo: "loads", pathMatch: "full"},
    ]
  }
];

