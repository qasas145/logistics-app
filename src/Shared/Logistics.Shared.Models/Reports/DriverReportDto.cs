namespace Logistics.Shared.Models.Reports;

public class DriverReportDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime JoinedDate { get; set; }
    
    // Truck Information
    public string? CurrentTruckNumber { get; set; }
    public string? TruckModel { get; set; }
    public string? TruckStatus { get; set; }
    
    // Load Statistics
    public int TotalLoadsCompleted { get; set; }
    public int TotalLoadsInProgress { get; set; }
    public int TotalLoadsDispatched { get; set; }
    public double TotalDistanceDriven { get; set; }
    public double AverageDistancePerLoad { get; set; }
    
    // Performance Metrics
    public decimal TotalEarnings { get; set; }
    public decimal AverageEarningsPerLoad { get; set; }
    public decimal AverageEarningsPerKm { get; set; }
    public int OnTimeDeliveryCount { get; set; }
    public int LateDeliveryCount { get; set; }
    public decimal OnTimeDeliveryPercentage { get; set; }
    public double AverageDeliveryTimeInHours { get; set; }
    
    // Period-based Statistics
    public DriverPeriodStatsDto ThisWeek { get; set; } = new();
    public DriverPeriodStatsDto LastWeek { get; set; } = new();
    public DriverPeriodStatsDto ThisMonth { get; set; } = new();
    public DriverPeriodStatsDto LastMonth { get; set; } = new();
    public DriverPeriodStatsDto ThisYear { get; set; } = new();
    
    // Recent Activity
    public List<LoadReportDto> RecentLoads { get; set; } = new();
    public DateTime? LastActiveDate { get; set; }
    public string? LastKnownLocation { get; set; }
}

public class DriverPeriodStatsDto
{
    public int LoadsCompleted { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TotalRevenue { get; set; }
    public double TotalDistance { get; set; }
    public decimal AverageEarningsPerLoad { get; set; }
    public int OnTimeDeliveries { get; set; }
    public int LateDeliveries { get; set; }
    public decimal OnTimePercentage { get; set; }
}