Reporting Feature - Loads, Drivers, Financials

Overview
This change adds a complete reporting capability across backend and frontend:
- Backend: New CQRS queries/handlers and an API controller under route `reports`.
- Shared: New DTOs and queries for loads, drivers, and financials reports.
- Frontend (Admin): New Blazor pages under Reports menu to visualize and filter reports.

Backend
- Added DTOs and queries:
  - `LoadsReportDto`, `LoadsReportItemDto`, `LoadsReportQuery`
  - `DriversReportDto`, `DriversReportItemDto`, `DriversReportQuery`
  - `FinancialsReportDto`, `FinancialsReportItemDto`, `FinancialsReportQuery`
- Implemented handlers:
  - `LoadsReportHandler`: aggregates and pages loads with totals.
  - `DriversReportHandler`: aggregates per-driver stats (loads delivered, distance, gross).
  - `FinancialsReportHandler`: aggregates invoices with totals paid/due.
- Added `ReportController` with endpoints:
  - GET `reports/loads`
  - GET `reports/drivers`
  - GET `reports/financials`
  All protected by `Permissions.Stats.View`.

Frontend (Admin)
- Navigation: Added Reports group to `MainLayout.razor` with three items.
- Pages:
  - `/reports/loads`: filters by date range and search; grid of loads with totals.
  - `/reports/drivers`: filters by date range and search; per-driver metrics with totals.
  - `/reports/financials`: filters by date range and search; invoice metrics with totals.

HTTP Client
- Added `IReportsApi` and implemented methods in `ApiClient`:
  - `GetLoadsReportAsync`, `GetDriversReportAsync`, `GetFinancialsReportAsync`
- Extended `IApiClient` to include `IReportsApi`.

How to Run
1) Ensure .NET SDK is installed and available as `dotnet`.
2) From repository root:
   - Build: `dotnet build Logistics.sln`
   - Run API: `dotnet run --project src/Presentation/Logistics.API/Logistics.API.csproj`
   - Run Admin (WASM): Serve via `dotnet run` in identity/API and use existing hosting or static server.

Endpoints
- GET `/reports/loads?from=...&to=...&search=...&status=...&page=1&pageSize=10`
- GET `/reports/drivers?from=...&to=...&search=...&page=1&pageSize=10`
- GET `/reports/financials?from=...&to=...&status=...&search=...&page=1&pageSize=10`

Notes
- Aggregations use existing entities via `ITenantUnitOfWork` and repository `Query()` access.
- Permissions align with existing `Stats.View` policy.

