using Logistics.Application.Queries.Reports.GetDriverReport;
using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications.Reports;

public sealed class DriverReportSpecification : BaseSpecification<Employee>
{
    public DriverReportSpecification(GetDriverReportQuery query)
    {
        // Filter by specific driver if requested
        if (query.DriverId.HasValue)
        {
            Criteria = e => e.Id == query.DriverId.Value;
        }

        // Additional filters can be added here based on query parameters
        // For now, we'll rely on the handler to filter by role since roles are a navigation property
    }
}