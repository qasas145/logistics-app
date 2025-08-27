namespace Logistics.Shared.Models;

public class DriversReportItemDto
{
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public int LoadsDelivered { get; set; }
    public double DistanceDriven { get; set; }
    public decimal GrossEarnings { get; set; }
}

public class DriversReportDto
{
    public IReadOnlyList<DriversReportItemDto> Items { get; set; } = Array.Empty<DriversReportItemDto>();
    public int TotalCount { get; set; }
    public decimal TotalGross { get; set; }
    public double TotalDistance { get; set; }
}

public class DriversReportQuery : PagedIntervalQuery
{
    public string? Search { get; set; }
}

