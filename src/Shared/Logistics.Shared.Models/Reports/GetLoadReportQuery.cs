namespace Logistics.Shared.Models.Reports;

public class GetLoadReportQuery : PagedQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public string? LoadType { get; set; }
    public Guid? AssignedDriverId { get; set; }
    public Guid? AssignedTruckId { get; set; }
    public Guid? CustomerId { get; set; }
    public decimal? MinDeliveryCost { get; set; }
    public decimal? MaxDeliveryCost { get; set; }
    public bool? IncludeInvoiceDetails { get; set; } = true;
    public string? SortBy { get; set; } = "DispatchedDate";
    public string? SortOrder { get; set; } = "desc";
}

public class GetLoadReportSummaryQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public string? LoadType { get; set; }
    public Guid? AssignedDriverId { get; set; }
    public Guid? AssignedTruckId { get; set; }
    public Guid? CustomerId { get; set; }
}

public class LoadReportSummaryDto
{
    public int TotalLoads { get; set; }
    public int CompletedLoads { get; set; }
    public int InProgressLoads { get; set; }
    public int DispatchedLoads { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalDriverPayouts { get; set; }
    public decimal TotalDistance { get; set; }
    public decimal AverageDeliveryCost { get; set; }
    public double AverageDistance { get; set; }
    public double AverageDeliveryTime { get; set; }
    public decimal OnTimeDeliveryPercentage { get; set; }
    public List<LoadsByStatusDto> LoadsByStatus { get; set; } = new();
    public List<LoadsByTypeDto> LoadsByType { get; set; } = new();
}

public class LoadsByStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Revenue { get; set; }
    public decimal Percentage { get; set; }
}

public class LoadsByTypeDto
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Revenue { get; set; }
    public decimal Percentage { get; set; }
}