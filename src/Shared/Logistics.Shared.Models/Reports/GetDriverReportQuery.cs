namespace Logistics.Shared.Models.Reports;

public class GetDriverReportQuery : PaginatedQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? TruckId { get; set; }
    public string? Status { get; set; }
    public bool? IncludeRecentLoads { get; set; } = true;
    public int? RecentLoadsLimit { get; set; } = 10;
    public string? SortBy { get; set; } = "TotalEarnings";
    public string? SortOrder { get; set; } = "desc";
}

public class GetDriverReportSummaryQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? DriverId { get; set; }
}

public class DriverReportSummaryDto
{
    public int TotalDrivers { get; set; }
    public int ActiveDrivers { get; set; }
    public int InactiveDrivers { get; set; }
    public decimal TotalDriverEarnings { get; set; }
    public decimal AverageDriverEarnings { get; set; }
    public double TotalDistanceDriven { get; set; }
    public double AverageDistancePerDriver { get; set; }
    public int TotalLoadsCompleted { get; set; }
    public decimal AverageLoadsPerDriver { get; set; }
    public decimal OverallOnTimePercentage { get; set; }
    public List<TopDriverDto> TopPerformers { get; set; } = new();
    public List<DriverEfficiencyDto> DriverEfficiency { get; set; } = new();
}

public class TopDriverDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalEarnings { get; set; }
    public int LoadsCompleted { get; set; }
    public double TotalDistance { get; set; }
    public decimal OnTimePercentage { get; set; }
}

public class DriverEfficiencyDto
{
    public string EfficiencyRange { get; set; } = string.Empty; // "High", "Medium", "Low"
    public int DriverCount { get; set; }
    public decimal AverageEarnings { get; set; }
    public decimal AverageOnTimePercentage { get; set; }
}