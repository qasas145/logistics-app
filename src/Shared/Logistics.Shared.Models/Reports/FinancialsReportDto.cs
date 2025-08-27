using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class FinancialsReportItemDto
{
    public Guid InvoiceId { get; set; }
    public long InvoiceNumber { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal Due => Total - Paid;
    public DateTime? DueDate { get; set; }
    public string? CustomerName { get; set; }
}

public class FinancialsReportDto
{
    public IReadOnlyList<FinancialsReportItemDto> Items { get; set; } = Array.Empty<FinancialsReportItemDto>();
    public int TotalCount { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalDue { get; set; }
}

public class FinancialsReportQuery : PagedIntervalQuery
{
    public InvoiceStatus? Status { get; set; }
    public string? Search { get; set; }
}

