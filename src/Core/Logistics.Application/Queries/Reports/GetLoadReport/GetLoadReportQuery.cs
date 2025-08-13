using Logistics.Shared.Models;
using Logistics.Shared.Models.Reports;
using MediatR;

namespace Logistics.Application.Queries.Reports.GetLoadReport;

public class GetLoadReportQuery : PagedQuery, IRequest<Result<PagedResult<LoadReportDto>>>
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