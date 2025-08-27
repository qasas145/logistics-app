using System.Text;
using Logistics.Infrastructure.Reporting;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Reporting;

public class ReportExportService : IReportExportService
{
    public Task<(byte[] Content, string ContentType, string FileName)> ExportLoadsAsync(LoadsReportDto data, string format, CancellationToken ct = default)
    {
        return ExportAsync("loads-report", data.Items.Select(i => new object?[] { i.Number, i.Name, i.Status, i.CreatedAt, i.DeliveredAt, i.TruckNumber, i.CustomerName, i.DeliveryCost, i.Distance }), format,
            ["#", "Name", "Status", "Created", "Delivered", "Truck", "Customer", "Revenue", "Distance"]);
    }

    public Task<(byte[] Content, string ContentType, string FileName)> ExportDriversAsync(DriversReportDto data, string format, CancellationToken ct = default)
    {
        return ExportAsync("drivers-report", data.Items.Select(i => new object?[] { i.DriverName, i.LoadsDelivered, i.DistanceDriven, i.GrossEarnings }), format,
            ["Driver", "Loads Delivered", "Distance", "Gross"]);
    }

    public Task<(byte[] Content, string ContentType, string FileName)> ExportFinancialsAsync(FinancialsReportDto data, string format, CancellationToken ct = default)
    {
        return ExportAsync("financials-report", data.Items.Select(i => new object?[] { i.InvoiceNumber, i.CustomerName, i.Status, i.Total, i.Paid, i.Due, i.DueDate }), format,
            ["Invoice #", "Customer", "Status", "Total", "Paid", "Due", "Due Date"]);
    }

    private Task<(byte[] Content, string ContentType, string FileName)> ExportAsync(string baseName, IEnumerable<object?[]> rows, string format, string[] headers)
    {
        format = format.ToLowerInvariant();
        return format switch
        {
            "csv" => Task.FromResult(ExportCsv(baseName, headers, rows)),
            "xlsx" or "excel" => Task.FromResult(ExportCsv(baseName, headers, rows)),
            "pdf" => Task.FromResult(ExportCsv(baseName, headers, rows)),
            "docx" or "word" => Task.FromResult(ExportCsv(baseName, headers, rows)),
            _ => Task.FromResult(ExportCsv(baseName, headers, rows))
        };
    }

    // Placeholder CSV until adding heavy deps like ClosedXML/QuestPDF/OpenXML
    private (byte[] Content, string ContentType, string FileName) ExportCsv(string baseName, string[] headers, IEnumerable<object?[]> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));
        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", row.Select(EscapeCsv)));
        }
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return (bytes, "text/csv", $"{baseName}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private static string EscapeCsv(object? value)
    {
        if (value is null) return "";
        var s = value switch
        {
            DateTime dt => dt.ToString("yyyy-MM-dd"),
            DateTimeOffset dto => dto.ToString("yyyy-MM-dd"),
            _ => value.ToString() ?? string.Empty
        };
        if (s.Contains('"') || s.Contains(',') || s.Contains('\n'))
        {
            s = '"' + s.Replace("\"", "\"\"") + '"';
        }
        return s;
    }
}

