using Logistics.Shared.Models;
using Logistics.Shared.Models.Reports;
using MediatR;

namespace Logistics.Application.Queries.Reports.GetCashFlowReport;

public class GetCashFlowReportQuery : IRequest<Result<CashFlowReportDto>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Period { get; set; } = "Monthly"; // Daily, Weekly, Monthly
}