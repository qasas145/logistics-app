using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public class LoadsReportItemDto
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public string? Name { get; set; }
    public LoadStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    public string? TruckNumber { get; set; }
    public string? CustomerName { get; set; }
}

public class LoadsReportDto
{
    public IReadOnlyList<LoadsReportItemDto> Items { get; set; } = Array.Empty<LoadsReportItemDto>();
    public int TotalCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public double TotalDistance { get; set; }
}

public class LoadsReportQuery : PagedIntervalQuery
{
    public string? Search { get; set; }
    public LoadStatus? Status { get; set; }
}

