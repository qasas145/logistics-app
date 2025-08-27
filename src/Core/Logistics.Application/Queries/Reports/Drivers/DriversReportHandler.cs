using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class DriversReportHandler(ITenantUnitOfWork tenantUow) : IAppRequestHandler<DriversReportQuery, Result<DriversReportDto>>
{
    public async Task<Result<DriversReportDto>> Handle(DriversReportQuery req, CancellationToken ct)
    {
        var employees = tenantUow.Repository<Employee>().Query();
        var trucks = tenantUow.Repository<Truck>().Query();
        var loads = tenantUow.Repository<Load>().Query();

        if (req.StartDate != default)
        {
            var from = req.StartDate;
            loads = loads.Where(l => l.CreatedAt >= from);
        }
        if (req.EndDate != default)
        {
            var to = req.EndDate;
            loads = loads.Where(l => l.CreatedAt <= to);
        }

        var driverStats = trucks
            .SelectMany(t => new[] { t.MainDriver, t.SecondaryDriver }.Where(d => d != null).Select(d => new { Truck = t, Driver = d! }))
            .Select(x => new DriversReportItemDto
            {
                DriverId = x.Driver.Id,
                DriverName = x.Driver.GetFullName(),
                LoadsDelivered = loads.Count(l => l.AssignedTruckId == x.Truck.Id && l.Status == Domain.Primitives.Enums.LoadStatus.Delivered),
                DistanceDriven = loads.Where(l => l.AssignedTruckId == x.Truck.Id && l.Status == Domain.Primitives.Enums.LoadStatus.Delivered).Sum(l => l.Distance),
                GrossEarnings = loads.Where(l => l.AssignedTruckId == x.Truck.Id && l.Status == Domain.Primitives.Enums.LoadStatus.Delivered).Sum(l => l.DeliveryCost)
            });

        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var term = req.Search.ToLower();
            driverStats = driverStats.Where(d => d.DriverName.ToLower().Contains(term));
        }

        var totalCount = driverStats.Count();
        var totalGross = driverStats.Sum(d => d.GrossEarnings);
        var totalDistance = driverStats.Sum(d => d.DistanceDriven);

        var items = driverStats
            .OrderByDescending(d => d.GrossEarnings)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToList();

        var dto = new DriversReportDto
        {
            Items = items,
            TotalCount = totalCount,
            TotalGross = totalGross,
            TotalDistance = totalDistance
        };

        return Result<DriversReportDto>.Ok(dto);
    }
}

