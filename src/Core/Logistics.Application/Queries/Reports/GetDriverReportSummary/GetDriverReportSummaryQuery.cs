using Logistics.Shared.Models;
using Logistics.Shared.Models.Reports;
using MediatR;

namespace Logistics.Application.Queries.Reports.GetDriverReportSummary;

public class GetDriverReportSummaryQuery : IRequest<Result<DriverReportSummaryDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? DriverId { get; set; }
}