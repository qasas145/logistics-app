using System.Text;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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
            "xlsx" or "excel" => Task.FromResult(ExportXlsx(baseName, headers, rows)),
            "pdf" => Task.FromResult(ExportPdf(baseName, headers, rows)),
            "docx" or "word" => Task.FromResult(ExportDocx(baseName, headers, rows)),
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

    private (byte[] Content, string ContentType, string FileName) ExportXlsx(string baseName, string[] headers, IEnumerable<object?[]> rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Report");

        for (int c = 0; c < headers.Length; c++)
        {
            ws.Cell(1, c + 1).Value = headers[c];
            ws.Cell(1, c + 1).Style.Font.SetBold();
        }

        var r = 2;
        foreach (var row in rows)
        {
            for (int c = 0; c < row.Length; c++)
            {
                var val = row[c];
                if (val is DateTime dt)
                    ws.Cell(r, c + 1).Value = dt;
                else if (val is DateTimeOffset dto)
                    ws.Cell(r, c + 1).Value = dto.DateTime;
                else if (val is int i)
                    ws.Cell(r, c + 1).Value = i;
                else if (val is long l)
                    ws.Cell(r, c + 1).Value = l;
                else if (val is double d)
                    ws.Cell(r, c + 1).Value = d;
                else if (val is decimal m)
                    ws.Cell(r, c + 1).Value = (double)m;
                else
                    ws.Cell(r, c + 1).Value = val?.ToString() ?? string.Empty;
            }
            r++;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return (ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{baseName}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }

    private (byte[] Content, string ContentType, string FileName) ExportPdf(string baseName, string[] headers, IEnumerable<object?[]> rows)
    {
        var data = rows.ToList();
        QuestPDF.Settings.License = LicenseType.Community;
        var doc = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Header().Text(baseName).SemiBold().FontSize(16);
                page.Content().Table(table =>
                {
                    var cols = headers.Length;
                    table.ColumnsDefinition(cd =>
                    {
                        for (int i = 0; i < cols; i++) cd.RelativeColumn();
                    });
                    table.Header(header =>
                    {
                        for (int i = 0; i < headers.Length; i++)
                            header.Cell().Element(CellStyle).Text(headers[i]);
                    });
                    foreach (var row in data)
                    {
                        for (int i = 0; i < row.Length; i++)
                            table.Cell().Element(CellStyle).Text(row[i]?.ToString() ?? string.Empty);
                    }

                    static IContainer CellStyle(IContainer container) => container.Padding(4).Border(1).BorderColor(Colors.Grey.Medium);
                });
            });
        });
        using var ms = new MemoryStream();
        doc.GeneratePdf(ms);
        return (ms.ToArray(), "application/pdf", $"{baseName}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
    }

    private (byte[] Content, string ContentType, string FileName) ExportDocx(string baseName, string[] headers, IEnumerable<object?[]> rows)
    {
        using var ms = new MemoryStream();
        using (var wordDoc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new Body());
            var body = mainPart.Document.Body!;

            var table = new Table();
            var headerRow = new TableRow();
            foreach (var h in headers)
            {
                headerRow.AppendChild(new TableCell(new Paragraph(new Run(new Text(h)))));
            }
            table.AppendChild(headerRow);

            foreach (var row in rows)
            {
                var tr = new TableRow();
                foreach (var cell in row)
                {
                    tr.AppendChild(new TableCell(new Paragraph(new Run(new Text(cell?.ToString() ?? string.Empty)))));
                }
                table.AppendChild(tr);
            }

            body.AppendChild(table);
            mainPart.Document.Save();
        }
        return (ms.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"{baseName}-{DateTime.UtcNow:yyyyMMddHHmmss}.docx");
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

