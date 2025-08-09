using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Reports;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries.Reports.GetDriverReportSummary;

public sealed class GetDriverReportSummaryHandler : IRequestHandler<GetDriverReportSummaryQuery, Result<DriverReportSummaryDto>>
{
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<Load> _loadRepository;
    private readonly IRepository<Truck> _truckRepository;

    public GetDriverReportSummaryHandler(
        IRepository<Employee> employeeRepository,
        IRepository<Load> loadRepository,
        IRepository<Truck> truckRepository)
    {
        _employeeRepository = employeeRepository;
        _loadRepository = loadRepository;
        _truckRepository = truckRepository;
    }

    public async Task<Result<DriverReportSummaryDto>> Handle(GetDriverReportSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all drivers (employees with driver role)
            var allDrivers = await _employeeRepository.GetAll()
                .Include(e => e.Roles)
                .Where(e => e.Roles.Any(r => r.Name.Contains("Driver", StringComparison.OrdinalIgnoreCase)))
                .ToListAsync(cancellationToken);

            // Get trucks to determine active drivers
            var trucks = await _truckRepository.GetAll()
                .Where(t => t.MainDriverId != null || t.SecondaryDriverId != null)
                .ToListAsync(cancellationToken);

            var activeDriverIds = trucks
                .SelectMany(t => new[] { t.MainDriverId, t.SecondaryDriverId })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            // Get loads within the date range
            var loadsQuery = _loadRepository.GetAll()
                .Include(l => l.AssignedTruck);

            if (request.StartDate.HasValue)
                loadsQuery = loadsQuery.Where(l => l.DispatchedDate >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                loadsQuery = loadsQuery.Where(l => l.DispatchedDate <= request.EndDate.Value);

            var loads = await loadsQuery.ToListAsync(cancellationToken);

            // Calculate driver statistics
            var driverLoads = loads
                .Where(l => l.AssignedTruck?.MainDriverId != null)
                .GroupBy(l => l.AssignedTruck!.MainDriverId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var summary = new DriverReportSummaryDto
            {
                TotalDrivers = allDrivers.Count,
                ActiveDrivers = activeDriverIds.Count,
                InactiveDrivers = allDrivers.Count - activeDriverIds.Count,
                TotalDriverEarnings = loads.Sum(l => l.CalcDriverShare()),
                TotalDistanceDriven = loads.Sum(l => l.Distance),
                TotalLoadsCompleted = loads.Count(l => l.Status == LoadStatus.Delivered)
            };

            // Calculate averages
            if (summary.TotalDrivers > 0)
            {
                summary.AverageDriverEarnings = summary.TotalDriverEarnings / summary.TotalDrivers;
                summary.AverageDistancePerDriver = summary.TotalDistanceDriven / summary.TotalDrivers;
                summary.AverageLoadsPerDriver = (decimal)summary.TotalLoadsCompleted / summary.TotalDrivers;
            }

            // Calculate overall on-time percentage
            var completedLoads = loads.Where(l => l.Status == LoadStatus.Delivered).ToList();
            if (completedLoads.Count > 0)
            {
                var onTimeDeliveries = completedLoads.Count(l => IsOnTimeDelivery(l));
                summary.OverallOnTimePercentage = (decimal)onTimeDeliveries / completedLoads.Count * 100;
            }

            // Calculate top performers
            summary.TopPerformers = driverLoads
                .Select(kvp => new
                {
                    DriverId = kvp.Key,
                    Loads = kvp.Value,
                    TotalEarnings = kvp.Value.Sum(l => l.CalcDriverShare()),
                    LoadsCompleted = kvp.Value.Count(l => l.Status == LoadStatus.Delivered),
                    TotalDistance = kvp.Value.Sum(l => l.Distance),
                    OnTimeDeliveries = kvp.Value.Count(l => l.Status == LoadStatus.Delivered && IsOnTimeDelivery(l))
                })
                .OrderByDescending(x => x.TotalEarnings)
                .Take(5)
                .Select(x => new TopDriverDto
                {
                    Id = x.DriverId,
                    Name = allDrivers.FirstOrDefault(d => d.Id == x.DriverId)?.GetFullName() ?? "Unknown",
                    TotalEarnings = x.TotalEarnings,
                    LoadsCompleted = x.LoadsCompleted,
                    TotalDistance = x.TotalDistance,
                    OnTimePercentage = x.LoadsCompleted > 0 ? (decimal)x.OnTimeDeliveries / x.LoadsCompleted * 100 : 0
                })
                .ToList();

            // Calculate efficiency distribution
            var driverEfficiencies = driverLoads
                .Select(kvp => new
                {
                    DriverId = kvp.Key,
                    OnTimePercentage = CalculateOnTimePercentage(kvp.Value),
                    AverageEarnings = kvp.Value.Sum(l => l.CalcDriverShare())
                })
                .ToList();

            summary.DriverEfficiency = new List<DriverEfficiencyDto>
            {
                new()
                {
                    EfficiencyRange = "High (>= 90%)",
                    DriverCount = driverEfficiencies.Count(d => d.OnTimePercentage >= 90),
                    AverageEarnings = driverEfficiencies.Where(d => d.OnTimePercentage >= 90).DefaultIfEmpty().Average(d => d?.AverageEarnings ?? 0),
                    AverageOnTimePercentage = driverEfficiencies.Where(d => d.OnTimePercentage >= 90).DefaultIfEmpty().Average(d => d?.OnTimePercentage ?? 0)
                },
                new()
                {
                    EfficiencyRange = "Medium (70-89%)",
                    DriverCount = driverEfficiencies.Count(d => d.OnTimePercentage >= 70 && d.OnTimePercentage < 90),
                    AverageEarnings = driverEfficiencies.Where(d => d.OnTimePercentage >= 70 && d.OnTimePercentage < 90).DefaultIfEmpty().Average(d => d?.AverageEarnings ?? 0),
                    AverageOnTimePercentage = driverEfficiencies.Where(d => d.OnTimePercentage >= 70 && d.OnTimePercentage < 90).DefaultIfEmpty().Average(d => d?.OnTimePercentage ?? 0)
                },
                new()
                {
                    EfficiencyRange = "Low (< 70%)",
                    DriverCount = driverEfficiencies.Count(d => d.OnTimePercentage < 70),
                    AverageEarnings = driverEfficiencies.Where(d => d.OnTimePercentage < 70).DefaultIfEmpty().Average(d => d?.AverageEarnings ?? 0),
                    AverageOnTimePercentage = driverEfficiencies.Where(d => d.OnTimePercentage < 70).DefaultIfEmpty().Average(d => d?.OnTimePercentage ?? 0)
                }
            };

            return Result<DriverReportSummaryDto>.Succeed(summary);
        }
        catch (Exception ex)
        {
            return Result<DriverReportSummaryDto>.Fail($"Error generating driver report summary: {ex.Message}");
        }
    }

    private static bool IsOnTimeDelivery(Load load)
    {
        if (!load.DeliveryDate.HasValue) return false;
        var expectedDeliveryTime = load.DispatchedDate.AddHours(load.Distance / 60);
        return load.DeliveryDate.Value <= expectedDeliveryTime.AddHours(24);
    }

    private static decimal CalculateOnTimePercentage(List<Load> loads)
    {
        var completedLoads = loads.Where(l => l.Status == LoadStatus.Delivered).ToList();
        if (completedLoads.Count == 0) return 0;

        var onTimeCount = completedLoads.Count(IsOnTimeDelivery);
        return (decimal)onTimeCount / completedLoads.Count * 100;
    }
}