using Logistics.Application.Queries.Reports.GetDriverReport;
using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using System.Linq.Expressions;

namespace Logistics.Application.Specifications.Reports;

public sealed class DriverReportSpecification : BaseSpecification<Employee>
{
    public DriverReportSpecification(GetDriverReportQuery query) : base(BuildCriteria(query))
    {
    }

    private static Expression<Func<Employee, bool>> BuildCriteria(GetDriverReportQuery query)
    {
        var criteria = PredicateBuilder.New<Employee>(true);

        // Filter by specific driver if requested
        if (query.DriverId.HasValue)
        {
            criteria = criteria.And(e => e.Id == query.DriverId.Value);
        }

        // Additional filters can be added here based on query parameters
        // For now, we'll rely on the handler to filter by role since roles are a navigation property

        return criteria;
    }
}