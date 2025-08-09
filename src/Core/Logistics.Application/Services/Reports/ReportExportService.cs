using Logistics.Shared.Models.Reports;
using System.Text;

namespace Logistics.Application.Services.Reports;

public class ReportExportService : IReportExportService
{
    public async Task<byte[]> ExportLoadReportToPdfAsync(List<LoadReportDto> loads, LoadReportSummaryDto? summary = null)
    {
        // For now, we'll generate a simple HTML report that can be converted to PDF
        // In a real implementation, you would use libraries like iTextSharp, PdfSharp, or similar
        
        var html = GenerateLoadReportHtml(loads, summary);
        
        // Simulate PDF generation by returning HTML as bytes
        // TODO: Replace with actual PDF generation using a PDF library
        return Encoding.UTF8.GetBytes(html);
    }

    public async Task<byte[]> ExportLoadReportToExcelAsync(List<LoadReportDto> loads, LoadReportSummaryDto? summary = null)
    {
        // For now, we'll generate CSV data that can be opened in Excel
        // In a real implementation, you would use libraries like EPPlus, ClosedXML, or similar
        
        var csv = GenerateLoadReportCsv(loads, summary);
        
        // TODO: Replace with actual Excel generation using an Excel library
        return Encoding.UTF8.GetBytes(csv);
    }

    public async Task<byte[]> ExportDriverReportToPdfAsync(List<DriverReportDto> drivers, DriverReportSummaryDto? summary = null)
    {
        var html = GenerateDriverReportHtml(drivers, summary);
        return Encoding.UTF8.GetBytes(html);
    }

    public async Task<byte[]> ExportDriverReportToExcelAsync(List<DriverReportDto> drivers, DriverReportSummaryDto? summary = null)
    {
        var csv = GenerateDriverReportCsv(drivers, summary);
        return Encoding.UTF8.GetBytes(csv);
    }

    public async Task<byte[]> ExportFinancialReportToPdfAsync(FinancialReportDto report)
    {
        var html = GenerateFinancialReportHtml(report);
        return Encoding.UTF8.GetBytes(html);
    }

    public async Task<byte[]> ExportFinancialReportToExcelAsync(FinancialReportDto report)
    {
        var csv = GenerateFinancialReportCsv(report);
        return Encoding.UTF8.GetBytes(csv);
    }

    public async Task<byte[]> ExportDashboardReportToPdfAsync(DashboardReportDto dashboard)
    {
        var html = GenerateDashboardReportHtml(dashboard);
        return Encoding.UTF8.GetBytes(html);
    }

    public async Task<byte[]> ExportDashboardReportToExcelAsync(DashboardReportDto dashboard)
    {
        var csv = GenerateDashboardReportCsv(dashboard);
        return Encoding.UTF8.GetBytes(csv);
    }

    private string GenerateLoadReportHtml(List<LoadReportDto> loads, LoadReportSummaryDto? summary)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html><head>");
        html.AppendLine("<title>Load Report</title>");
        html.AppendLine("<style>");
        html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
        html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        html.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
        html.AppendLine(".summary { background-color: #f8f9fa; padding: 15px; margin-bottom: 20px; border-radius: 5px; }");
        html.AppendLine("</style>");
        html.AppendLine("</head><body>");

        html.AppendLine("<h1>Load Report</h1>");
        html.AppendLine($"<p>Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

        if (summary != null)
        {
            html.AppendLine("<div class='summary'>");
            html.AppendLine("<h2>Summary</h2>");
            html.AppendLine($"<p><strong>Total Loads:</strong> {summary.TotalLoads:N0}</p>");
            html.AppendLine($"<p><strong>Completed Loads:</strong> {summary.CompletedLoads:N0}</p>");
            html.AppendLine($"<p><strong>Total Revenue:</strong> ${summary.TotalRevenue:N2}</p>");
            html.AppendLine($"<p><strong>On-Time Delivery:</strong> {summary.OnTimeDeliveryPercentage:F1}%</p>");
            html.AppendLine("</div>");
        }

        html.AppendLine("<table>");
        html.AppendLine("<tr>");
        html.AppendLine("<th>Load #</th><th>Name</th><th>Status</th><th>Origin</th><th>Destination</th>");
        html.AppendLine("<th>Cost</th><th>Driver</th><th>Dispatched Date</th><th>Payment Status</th>");
        html.AppendLine("</tr>");

        foreach (var load in loads)
        {
            html.AppendLine("<tr>");
            html.AppendLine($"<td>{load.Number}</td>");
            html.AppendLine($"<td>{load.Name}</td>");
            html.AppendLine($"<td>{load.Status}</td>");
            html.AppendLine($"<td>{load.OriginAddress}</td>");
            html.AppendLine($"<td>{load.DestinationAddress}</td>");
            html.AppendLine($"<td>${load.DeliveryCost:N2}</td>");
            html.AppendLine($"<td>{load.AssignedDriverName ?? "Unassigned"}</td>");
            html.AppendLine($"<td>{DateTime.Parse(load.DispatchedDate):yyyy-MM-dd}</td>");
            html.AppendLine($"<td>{(load.IsPaid ? "Paid" : "Pending")}</td>");
            html.AppendLine("</tr>");
        }

        html.AppendLine("</table>");
        html.AppendLine("</body></html>");

        return html.ToString();
    }

    private string GenerateLoadReportCsv(List<LoadReportDto> loads, LoadReportSummaryDto? summary)
    {
        var csv = new StringBuilder();
        
        csv.AppendLine("Load Report");
        csv.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine();

        if (summary != null)
        {
            csv.AppendLine("SUMMARY");
            csv.AppendLine($"Total Loads,{summary.TotalLoads}");
            csv.AppendLine($"Completed Loads,{summary.CompletedLoads}");
            csv.AppendLine($"Total Revenue,{summary.TotalRevenue:C}");
            csv.AppendLine($"On-Time Delivery %,{summary.OnTimeDeliveryPercentage:F1}");
            csv.AppendLine();
        }

        csv.AppendLine("LOAD DETAILS");
        csv.AppendLine("Load #,Name,Status,Type,Origin,Destination,Distance (km),Cost,Driver Share,Company Revenue,Driver,Truck,Dispatched Date,Delivery Date,Payment Status");

        foreach (var load in loads)
        {
            csv.AppendLine($"{load.Number},{EscapeCsvField(load.Name)},{load.Status},{load.Type}," +
                          $"{EscapeCsvField(load.OriginAddress)},{EscapeCsvField(load.DestinationAddress)}," +
                          $"{load.Distance:F1},{load.DeliveryCost:F2},{load.DriverShare:F2},{load.CompanyRevenue:F2}," +
                          $"{EscapeCsvField(load.AssignedDriverName ?? "")},{EscapeCsvField(load.AssignedTruckNumber ?? "")}," +
                          $"{DateTime.Parse(load.DispatchedDate):yyyy-MM-dd}," +
                          $"{(string.IsNullOrEmpty(load.DeliveryDate) ? "" : DateTime.Parse(load.DeliveryDate).ToString("yyyy-MM-dd"))}," +
                          $"{(load.IsPaid ? "Paid" : "Pending")}");
        }

        return csv.ToString();
    }

    private string GenerateDriverReportHtml(List<DriverReportDto> drivers, DriverReportSummaryDto? summary)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html><head>");
        html.AppendLine("<title>Driver Report</title>");
        html.AppendLine("<style>");
        html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
        html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        html.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
        html.AppendLine(".summary { background-color: #f8f9fa; padding: 15px; margin-bottom: 20px; border-radius: 5px; }");
        html.AppendLine("</style>");
        html.AppendLine("</head><body>");

        html.AppendLine("<h1>Driver Report</h1>");
        html.AppendLine($"<p>Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

        if (summary != null)
        {
            html.AppendLine("<div class='summary'>");
            html.AppendLine("<h2>Summary</h2>");
            html.AppendLine($"<p><strong>Total Drivers:</strong> {summary.TotalDrivers:N0}</p>");
            html.AppendLine($"<p><strong>Active Drivers:</strong> {summary.ActiveDrivers:N0}</p>");
            html.AppendLine($"<p><strong>Total Earnings:</strong> ${summary.TotalDriverEarnings:N2}</p>");
            html.AppendLine($"<p><strong>On-Time Delivery:</strong> {summary.OverallOnTimePercentage:F1}%</p>");
            html.AppendLine("</div>");
        }

        html.AppendLine("<table>");
        html.AppendLine("<tr>");
        html.AppendLine("<th>Driver Name</th><th>Email</th><th>Truck</th><th>Loads Completed</th>");
        html.AppendLine("<th>Total Earnings</th><th>Distance Driven</th><th>On-Time %</th><th>Joined Date</th>");
        html.AppendLine("</tr>");

        foreach (var driver in drivers)
        {
            html.AppendLine("<tr>");
            html.AppendLine($"<td>{driver.FullName}</td>");
            html.AppendLine($"<td>{driver.Email}</td>");
            html.AppendLine($"<td>{driver.CurrentTruckNumber ?? "N/A"}</td>");
            html.AppendLine($"<td>{driver.TotalLoadsCompleted:N0}</td>");
            html.AppendLine($"<td>${driver.TotalEarnings:N2}</td>");
            html.AppendLine($"<td>{driver.TotalDistanceDriven:N1} km</td>");
            html.AppendLine($"<td>{driver.OnTimeDeliveryPercentage:F1}%</td>");
            html.AppendLine($"<td>{DateTime.Parse(driver.JoinedDate):yyyy-MM-dd}</td>");
            html.AppendLine("</tr>");
        }

        html.AppendLine("</table>");
        html.AppendLine("</body></html>");

        return html.ToString();
    }

    private string GenerateDriverReportCsv(List<DriverReportDto> drivers, DriverReportSummaryDto? summary)
    {
        var csv = new StringBuilder();
        
        csv.AppendLine("Driver Report");
        csv.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine();

        if (summary != null)
        {
            csv.AppendLine("SUMMARY");
            csv.AppendLine($"Total Drivers,{summary.TotalDrivers}");
            csv.AppendLine($"Active Drivers,{summary.ActiveDrivers}");
            csv.AppendLine($"Total Earnings,{summary.TotalDriverEarnings:C}");
            csv.AppendLine($"Overall On-Time %,{summary.OverallOnTimePercentage:F1}");
            csv.AppendLine();
        }

        csv.AppendLine("DRIVER DETAILS");
        csv.AppendLine("Driver Name,Email,Phone,Truck Number,Truck Model,Total Loads,Completed Loads,Total Earnings,Average Earnings/Load,Distance Driven,On-Time Delivery %,Joined Date");

        foreach (var driver in drivers)
        {
            csv.AppendLine($"{EscapeCsvField(driver.FullName)},{driver.Email},{EscapeCsvField(driver.PhoneNumber ?? "")}," +
                          $"{EscapeCsvField(driver.CurrentTruckNumber ?? "")},{EscapeCsvField(driver.TruckModel ?? "")}," +
                          $"{driver.TotalLoadsCompleted + driver.TotalLoadsInProgress + driver.TotalLoadsDispatched}," +
                          $"{driver.TotalLoadsCompleted},{driver.TotalEarnings:F2},{driver.AverageEarningsPerLoad:F2}," +
                          $"{driver.TotalDistanceDriven:F1},{driver.OnTimeDeliveryPercentage:F1}," +
                          $"{DateTime.Parse(driver.JoinedDate):yyyy-MM-dd}");
        }

        return csv.ToString();
    }

    private string GenerateFinancialReportHtml(FinancialReportDto report)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html><head>");
        html.AppendLine("<title>Financial Report</title>");
        html.AppendLine("<style>");
        html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
        html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        html.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
        html.AppendLine(".summary { background-color: #f8f9fa; padding: 15px; margin-bottom: 20px; border-radius: 5px; }");
        html.AppendLine(".financial-section { margin-bottom: 30px; }");
        html.AppendLine("</style>");
        html.AppendLine("</head><body>");

        html.AppendLine("<h1>Financial Report</h1>");
        html.AppendLine($"<p>Period: {DateTime.Parse(report.PeriodStart):yyyy-MM-dd} to {DateTime.Parse(report.PeriodEnd):yyyy-MM-dd}</p>");
        html.AppendLine($"<p>Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

        html.AppendLine("<div class='financial-section'>");
        html.AppendLine("<h2>Financial Summary</h2>");
        html.AppendLine($"<p><strong>Total Revenue:</strong> ${report.Revenue.TotalRevenue:N2}</p>");
        html.AppendLine($"<p><strong>Total Expenses:</strong> ${report.Expenses.TotalExpenses:N2}</p>");
        html.AppendLine($"<p><strong>Gross Profit:</strong> ${report.GrossProfit:N2}</p>");
        html.AppendLine($"<p><strong>Net Profit:</strong> ${report.NetProfit:N2}</p>");
        html.AppendLine($"<p><strong>Profit Margin:</strong> {report.ProfitMargin:F1}%</p>");
        html.AppendLine("</div>");

        html.AppendLine("<div class='financial-section'>");
        html.AppendLine("<h2>Revenue Breakdown</h2>");
        html.AppendLine($"<p><strong>Load Revenue:</strong> ${report.Revenue.LoadRevenue:N2}</p>");
        html.AppendLine($"<p><strong>Loads Delivered:</strong> {report.Revenue.TotalLoadsDelivered:N0}</p>");
        html.AppendLine($"<p><strong>Average Revenue per Load:</strong> ${report.Revenue.AverageRevenuePerLoad:N2}</p>");
        html.AppendLine("</div>");

        html.AppendLine("<div class='financial-section'>");
        html.AppendLine("<h2>Expense Breakdown</h2>");
        html.AppendLine($"<p><strong>Driver Payouts:</strong> ${report.Expenses.DriverPayouts:N2}</p>");
        html.AppendLine($"<p><strong>Payroll Expenses:</strong> ${report.Expenses.PayrollExpenses:N2}</p>");
        html.AppendLine($"<p><strong>Operational Expenses:</strong> ${report.Expenses.OperationalExpenses:N2}</p>");
        html.AppendLine("</div>");

        html.AppendLine("</body></html>");

        return html.ToString();
    }

    private string GenerateFinancialReportCsv(FinancialReportDto report)
    {
        var csv = new StringBuilder();
        
        csv.AppendLine("Financial Report");
        csv.AppendLine($"Period: {DateTime.Parse(report.PeriodStart):yyyy-MM-dd} to {DateTime.Parse(report.PeriodEnd):yyyy-MM-dd}");
        csv.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine();

        csv.AppendLine("FINANCIAL SUMMARY");
        csv.AppendLine($"Total Revenue,{report.Revenue.TotalRevenue:F2}");
        csv.AppendLine($"Total Expenses,{report.Expenses.TotalExpenses:F2}");
        csv.AppendLine($"Gross Profit,{report.GrossProfit:F2}");
        csv.AppendLine($"Net Profit,{report.NetProfit:F2}");
        csv.AppendLine($"Profit Margin %,{report.ProfitMargin:F1}");
        csv.AppendLine();

        csv.AppendLine("REVENUE BREAKDOWN");
        csv.AppendLine($"Load Revenue,{report.Revenue.LoadRevenue:F2}");
        csv.AppendLine($"Fuel Surcharges,{report.Revenue.FuelSurcharges:F2}");
        csv.AppendLine($"Accessorial Charges,{report.Revenue.AccessorialCharges:F2}");
        csv.AppendLine($"Total Loads Delivered,{report.Revenue.TotalLoadsDelivered}");
        csv.AppendLine($"Average Revenue per Load,{report.Revenue.AverageRevenuePerLoad:F2}");
        csv.AppendLine();

        csv.AppendLine("EXPENSE BREAKDOWN");
        csv.AppendLine($"Driver Payouts,{report.Expenses.DriverPayouts:F2}");
        csv.AppendLine($"Payroll Expenses,{report.Expenses.PayrollExpenses:F2}");
        csv.AppendLine($"Fuel Costs,{report.Expenses.FuelCosts:F2}");
        csv.AppendLine($"Maintenance Costs,{report.Expenses.MaintenanceCosts:F2}");
        csv.AppendLine($"Operational Expenses,{report.Expenses.OperationalExpenses:F2}");

        return csv.ToString();
    }

    private string GenerateDashboardReportHtml(DashboardReportDto dashboard)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html><head>");
        html.AppendLine("<title>Dashboard Report</title>");
        html.AppendLine("<style>");
        html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        html.AppendLine(".summary { background-color: #f8f9fa; padding: 15px; margin-bottom: 20px; border-radius: 5px; }");
        html.AppendLine(".section { margin-bottom: 30px; }");
        html.AppendLine("</style>");
        html.AppendLine("</head><body>");

        html.AppendLine("<h1>Dashboard Report</h1>");
        html.AppendLine($"<p>Period: {DateTime.Parse(dashboard.PeriodStart):yyyy-MM-dd} to {DateTime.Parse(dashboard.PeriodEnd):yyyy-MM-dd}</p>");
        html.AppendLine($"<p>Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

        html.AppendLine("<div class='section'>");
        html.AppendLine("<h2>Load Summary</h2>");
        html.AppendLine($"<p><strong>Total Loads:</strong> {dashboard.LoadSummary.TotalLoads:N0}</p>");
        html.AppendLine($"<p><strong>Completed:</strong> {dashboard.LoadSummary.CompletedLoads:N0}</p>");
        html.AppendLine($"<p><strong>Revenue:</strong> ${dashboard.LoadSummary.TotalRevenue:N2}</p>");
        html.AppendLine("</div>");

        html.AppendLine("<div class='section'>");
        html.AppendLine("<h2>Driver Summary</h2>");
        html.AppendLine($"<p><strong>Total Drivers:</strong> {dashboard.DriverSummary.TotalDrivers:N0}</p>");
        html.AppendLine($"<p><strong>Active:</strong> {dashboard.DriverSummary.ActiveDrivers:N0}</p>");
        html.AppendLine($"<p><strong>Total Earnings:</strong> ${dashboard.DriverSummary.TotalDriverEarnings:N2}</p>");
        html.AppendLine("</div>");

        html.AppendLine("<div class='section'>");
        html.AppendLine("<h2>Financial Summary</h2>");
        html.AppendLine($"<p><strong>Revenue:</strong> ${dashboard.FinancialSummary.TotalRevenue:N2}</p>");
        html.AppendLine($"<p><strong>Expenses:</strong> ${dashboard.FinancialSummary.TotalExpenses:N2}</p>");
        html.AppendLine($"<p><strong>Profit:</strong> ${dashboard.FinancialSummary.NetProfit:N2}</p>");
        html.AppendLine("</div>");

        html.AppendLine("</body></html>");

        return html.ToString();
    }

    private string GenerateDashboardReportCsv(DashboardReportDto dashboard)
    {
        var csv = new StringBuilder();
        
        csv.AppendLine("Dashboard Report");
        csv.AppendLine($"Period: {DateTime.Parse(dashboard.PeriodStart):yyyy-MM-dd} to {DateTime.Parse(dashboard.PeriodEnd):yyyy-MM-dd}");
        csv.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine();

        csv.AppendLine("LOAD SUMMARY");
        csv.AppendLine($"Total Loads,{dashboard.LoadSummary.TotalLoads}");
        csv.AppendLine($"Completed Loads,{dashboard.LoadSummary.CompletedLoads}");
        csv.AppendLine($"Total Revenue,{dashboard.LoadSummary.TotalRevenue:F2}");
        csv.AppendLine();

        csv.AppendLine("DRIVER SUMMARY");
        csv.AppendLine($"Total Drivers,{dashboard.DriverSummary.TotalDrivers}");
        csv.AppendLine($"Active Drivers,{dashboard.DriverSummary.ActiveDrivers}");
        csv.AppendLine($"Total Driver Earnings,{dashboard.DriverSummary.TotalDriverEarnings:F2}");
        csv.AppendLine();

        csv.AppendLine("FINANCIAL SUMMARY");
        csv.AppendLine($"Total Revenue,{dashboard.FinancialSummary.TotalRevenue:F2}");
        csv.AppendLine($"Total Expenses,{dashboard.FinancialSummary.TotalExpenses:F2}");
        csv.AppendLine($"Net Profit,{dashboard.FinancialSummary.NetProfit:F2}");

        return csv.ToString();
    }

    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";

        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }

        return field;
    }
}