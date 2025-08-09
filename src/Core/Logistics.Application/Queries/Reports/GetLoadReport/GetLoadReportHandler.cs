using Logistics.Application.Specifications.Reports;
using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Reports;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries.Reports.GetLoadReport;

public sealed class GetLoadReportHandler : IRequestHandler<GetLoadReportQuery, Result<PagedResult<LoadReportDto>>>
{
    private readonly IRepository<Load> _loadRepository;

    public GetLoadReportHandler(IRepository<Load> loadRepository)
    {
        _loadRepository = loadRepository;
    }

    public async Task<Result<PagedResult<LoadReportDto>>> Handle(GetLoadReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var specification = new LoadReportSpecification(request);
            
            var query = _loadRepository.GetQueryableBySpec(specification)
                .Include(l => l.AssignedTruck)
                .Include(l => l.AssignedDispatcher)
                .Include(l => l.Customer)
                .Include(l => l.Invoices)
                .ThenInclude(i => i.Payments);

            // Apply sorting
            query = ApplySorting(query, request.SortBy, request.SortOrder);

            var totalCount = await query.CountAsync(cancellationToken);
            
            var loads = await query
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .ToListAsync(cancellationToken);

            var loadReports = loads.Select(load => LoadReportMapper.MapToLoadReportDto(load, request.IncludeInvoiceDetails ?? true)).ToList();

            var result = new PagedResult<LoadReportDto>
            {
                Items = loadReports,
                TotalCount = totalCount,
                Page = request.Page,
                Size = request.Size,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.Size)
            };

            return Result<PagedResult<LoadReportDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<LoadReportDto>>.Failure($"Error generating load report: {ex.Message}");
        }
    }

    private static IQueryable<Load> ApplySorting(IQueryable<Load> query, string? sortBy, string? sortOrder)
    {
        var isDescending = sortOrder?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "dispatcheddate" => isDescending ? query.OrderByDescending(l => l.DispatchedDate) : query.OrderBy(l => l.DispatchedDate),
            "deliverydate" => isDescending ? query.OrderByDescending(l => l.DeliveryDate) : query.OrderBy(l => l.DeliveryDate),
            "deliverycost" => isDescending ? query.OrderByDescending(l => l.DeliveryCost.Amount) : query.OrderBy(l => l.DeliveryCost.Amount),
            "distance" => isDescending ? query.OrderByDescending(l => l.Distance) : query.OrderBy(l => l.Distance),
            "status" => isDescending ? query.OrderByDescending(l => l.Status) : query.OrderBy(l => l.Status),
            "name" => isDescending ? query.OrderByDescending(l => l.Name) : query.OrderBy(l => l.Name),
            "number" => isDescending ? query.OrderByDescending(l => l.Number) : query.OrderBy(l => l.Number),
            _ => isDescending ? query.OrderByDescending(l => l.DispatchedDate) : query.OrderBy(l => l.DispatchedDate)
        };
    }
}