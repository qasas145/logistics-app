using Logistics.Shared.Models;
using Logistics.Shared.Models.Reports;
using MediatR;

namespace Logistics.Application.Queries.Reports.GetLoadReportSummary;

public class GetLoadReportSummaryQuery : IRequest<Result<LoadReportSummaryDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public string? LoadType { get; set; }
    public Guid? AssignedDriverId { get; set; }
    public Guid? AssignedTruckId { get; set; }
    public Guid? CustomerId { get; set; }
}