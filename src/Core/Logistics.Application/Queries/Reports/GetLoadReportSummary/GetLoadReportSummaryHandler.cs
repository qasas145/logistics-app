using Logistics.Application.Specifications.Reports;
using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Reports;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries.Reports.GetLoadReportSummary;

public sealed class GetLoadReportSummaryHandler : IRequestHandler<GetLoadReportSummaryQuery, Result<LoadReportSummaryDto>>
{
    private readonly IRepository<Load> _loadRepository;

    public GetLoadReportSummaryHandler(IRepository<Load> loadRepository)
    {
        _loadRepository = loadRepository;
    }

    public async Task<Result<LoadReportSummaryDto>> Handle(GetLoadReportSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var specification = new LoadReportSummarySpecification(request);
            
            var loads = await _loadRepository.GetQueryableBySpec(specification)
                .Include(l => l.AssignedTruck)
                .Include(l => l.Invoices)
                .ToListAsync(cancellationToken);

            var summary = new LoadReportSummaryDto
            {
                TotalLoads = loads.Count,
                CompletedLoads = loads.Count(l => l.Status == LoadStatus.Delivered),
                InProgressLoads = loads.Count(l => l.Status == LoadStatus.PickedUp),
                DispatchedLoads = loads.Count(l => l.Status == LoadStatus.Dispatched),
                TotalRevenue = loads.Sum(l => l.DeliveryCost.Amount),
                TotalDriverPayouts = loads.Sum(l => l.CalcDriverShare()),
                TotalDistance = loads.Sum(l => l.Distance),
                AverageDeliveryCost = loads.Count > 0 ? loads.Average(l => l.DeliveryCost.Amount) : 0,
                AverageDistance = loads.Count > 0 ? loads.Average(l => l.Distance) : 0,
            };

            // Calculate on-time delivery percentage
            var deliveredLoads = loads.Where(l => l.Status == LoadStatus.Delivered && l.DeliveryDate.HasValue && l.DispatchedDate != default).ToList();
            if (deliveredLoads.Count > 0)
            {
                var onTimeDeliveries = deliveredLoads.Count(l => IsOnTimeDelivery(l));
                summary.OnTimeDeliveryPercentage = (decimal)onTimeDeliveries / deliveredLoads.Count * 100;
                summary.AverageDeliveryTime = deliveredLoads.Average(l => 
                    l.DeliveryDate!.Value.Subtract(l.DispatchedDate).TotalHours);
            }

            // Group by status
            summary.LoadsByStatus = loads.GroupBy(l => l.Status)
                .Select(g => new LoadsByStatusDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Revenue = g.Sum(l => l.DeliveryCost.Amount),
                    Percentage = summary.TotalLoads > 0 ? (decimal)g.Count() / summary.TotalLoads * 100 : 0
                }).ToList();

            // Group by type
            summary.LoadsByType = loads.GroupBy(l => l.Type)
                .Select(g => new LoadsByTypeDto
                {
                    Type = g.Key.ToString(),
                    Count = g.Count(),
                    Revenue = g.Sum(l => l.DeliveryCost.Amount),
                    Percentage = summary.TotalLoads > 0 ? (decimal)g.Count() / summary.TotalLoads * 100 : 0
                }).ToList();

            return Result<LoadReportSummaryDto>.Succeed(summary);
        }
        catch (Exception ex)
        {
            return Result<LoadReportSummaryDto>.Fail($"Error generating load report summary: {ex.Message}");
        }
    }

    private static bool IsOnTimeDelivery(Load load)
    {
        // Consider a load on-time if delivered within 24 hours of expected delivery
        // This is a simplified calculation - in real scenarios, you might have specific delivery time windows
        if (!load.DeliveryDate.HasValue) return false;
        
        var expectedDeliveryTime = load.DispatchedDate.AddHours(load.Distance / 60); // Assume 60 km/h average speed
        return load.DeliveryDate.Value <= expectedDeliveryTime.AddHours(24);
    }
}