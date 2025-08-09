using Logistics.Shared.Models;
using Logistics.Shared.Models.Reports;
using MediatR;

namespace Logistics.Application.Queries.Reports.GetDriverReport;

public class GetDriverReportQuery : PagedQuery, IRequest<Result<PagedResult<DriverReportDto>>>
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