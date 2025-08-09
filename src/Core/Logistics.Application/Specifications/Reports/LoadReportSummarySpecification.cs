using Logistics.Application.Queries.Reports.GetLoadReportSummary;
using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using System.Linq.Expressions;

namespace Logistics.Application.Specifications.Reports;

public sealed class LoadReportSummarySpecification : BaseSpecification<Load>
{
    public LoadReportSummarySpecification(GetLoadReportSummaryQuery query) : base(BuildCriteria(query))
    {
    }

    private static Expression<Func<Load, bool>> BuildCriteria(GetLoadReportSummaryQuery query)
    {
        var criteria = PredicateBuilder.New<Load>(true);

        if (query.StartDate.HasValue)
        {
            criteria = criteria.And(l => l.DispatchedDate >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            criteria = criteria.And(l => l.DispatchedDate <= query.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (Enum.TryParse<LoadStatus>(query.Status, true, out var status))
            {
                criteria = criteria.And(l => l.Status == status);
            }
        }

        if (!string.IsNullOrWhiteSpace(query.LoadType))
        {
            if (Enum.TryParse<LoadType>(query.LoadType, true, out var loadType))
            {
                criteria = criteria.And(l => l.Type == loadType);
            }
        }

        if (query.AssignedDriverId.HasValue)
        {
            criteria = criteria.And(l => l.AssignedTruck != null && 
                                       l.AssignedTruck.DriverId == query.AssignedDriverId.Value);
        }

        if (query.AssignedTruckId.HasValue)
        {
            criteria = criteria.And(l => l.AssignedTruckId == query.AssignedTruckId.Value);
        }

        if (query.CustomerId.HasValue)
        {
            criteria = criteria.And(l => l.CustomerId == query.CustomerId.Value);
        }

        return criteria;
    }
}