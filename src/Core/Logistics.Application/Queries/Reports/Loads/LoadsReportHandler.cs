using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class LoadsReportHandler(ITenantUnitOfWork tenantUow) : IAppRequestHandler<LoadsReportQuery, Result<LoadsReportDto>>
{
    public async Task<Result<LoadsReportDto>> Handle(LoadsReportQuery req, CancellationToken ct)
    {
        var repository = tenantUow.Repository<Load>();

        var queryable = repository.Query();

        if (req.StartDate != default)
        {
            var from = req.StartDate;
            queryable = queryable.Where(l => l.CreatedAt >= from);
        }
        if (req.EndDate != default)
        {
            var to = req.EndDate;
            queryable = queryable.Where(l => l.CreatedAt <= to);
        }
        if (req.Status is not null)
        {
            queryable = queryable.Where(l => l.Status == req.Status);
        }
        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var term = req.Search.ToLower();
            queryable = queryable.Where(l => l.Name.ToLower().Contains(term) || (l.AssignedTruck != null && l.AssignedTruck.Number.ToLower().Contains(term)) || (l.Customer != null && l.Customer.Name.ToLower().Contains(term)));
        }

        var totalCount = queryable.Count();
        var totalRevenue = queryable.Select(l => l.DeliveryCost.Amount).Sum();
        var totalDistance = queryable.Sum(l => l.Distance);

        var items = queryable
            .OrderByDescending(l => l.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(l => new LoadsReportItemDto
            {
                Id = l.Id,
                Number = l.Number,
                Name = l.Name,
                Status = l.Status,
                CreatedAt = l.CreatedAt,
                DeliveredAt = l.DeliveredAt,
                DeliveryCost = l.DeliveryCost.Amount,
                Distance = l.Distance,
                TruckNumber = l.AssignedTruck != null ? l.AssignedTruck.Number : null,
                CustomerName = l.Customer != null ? l.Customer.Name : null
            })
            .ToList();

        var dto = new LoadsReportDto
        {
            Items = items,
            TotalCount = totalCount,
            TotalRevenue = totalRevenue,
            TotalDistance = totalDistance
        };

        return Result<LoadsReportDto>.Ok(dto);
    }
}

