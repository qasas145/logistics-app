using Logistics.Shared.Models;
using Logistics.Shared.Models.Reports;
using MediatR;

namespace Logistics.Application.Queries.Reports.GetFinancialReport;

public class GetFinancialReportQuery : IRequest<Result<FinancialReportDto>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string ReportType { get; set; } = "Summary"; // Summary, Detailed, Comparison
    public string Period { get; set; } = "Monthly"; // Daily, Weekly, Monthly, Quarterly, Yearly
    public bool IncludeComparison { get; set; } = true;
    public bool IncludeTopPerformers { get; set; } = true;
    public int TopPerformersLimit { get; set; } = 10;
    public bool IncludeInvoiceAging { get; set; } = true;
    public string? Currency { get; set; }
}