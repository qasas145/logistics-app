using Logistics.Shared.Models.Reports;

namespace Logistics.Application.Services.Reports;

public interface IReportExportService
{
    Task<byte[]> ExportLoadReportToPdfAsync(List<LoadReportDto> loads, LoadReportSummaryDto? summary = null);
    Task<byte[]> ExportLoadReportToExcelAsync(List<LoadReportDto> loads, LoadReportSummaryDto? summary = null);
    
    Task<byte[]> ExportDriverReportToPdfAsync(List<DriverReportDto> drivers, DriverReportSummaryDto? summary = null);
    Task<byte[]> ExportDriverReportToExcelAsync(List<DriverReportDto> drivers, DriverReportSummaryDto? summary = null);
    
    Task<byte[]> ExportFinancialReportToPdfAsync(FinancialReportDto report);
    Task<byte[]> ExportFinancialReportToExcelAsync(FinancialReportDto report);
    
    Task<byte[]> ExportDashboardReportToPdfAsync(DashboardReportDto dashboard);
    Task<byte[]> ExportDashboardReportToExcelAsync(DashboardReportDto dashboard);
}