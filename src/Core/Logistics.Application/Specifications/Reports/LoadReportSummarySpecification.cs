using Logistics.Application.Queries.Reports.GetLoadReportSummary;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications.Reports;

public sealed class LoadReportSummarySpecification : BaseSpecification<Load>
{
    public LoadReportSummarySpecification(GetLoadReportSummaryQuery query)
    {
        // Build criteria based on filters
        if (query.StartDate.HasValue && query.EndDate.HasValue)
        {
            Criteria = l => l.DispatchedDate >= query.StartDate.Value && 
                           l.DispatchedDate <= query.EndDate.Value;
        }
        else if (query.StartDate.HasValue)
        {
            Criteria = l => l.DispatchedDate >= query.StartDate.Value;
        }
        else if (query.EndDate.HasValue)
        {
            Criteria = l => l.DispatchedDate <= query.EndDate.Value;
        }

        // Apply additional filters using simplified approach
        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<LoadStatus>(query.Status, true, out var status))
        {
            var currentCriteria = Criteria;
            Criteria = currentCriteria == null ? 
                l => l.Status == status : 
                l => currentCriteria.Compile()(l) && l.Status == status;
        }

        if (!string.IsNullOrWhiteSpace(query.LoadType) && Enum.TryParse<LoadType>(query.LoadType, true, out var loadType))
        {
            var currentCriteria = Criteria;
            Criteria = currentCriteria == null ? 
                l => l.Type == loadType : 
                l => currentCriteria.Compile()(l) && l.Type == loadType;
        }

        if (query.AssignedDriverId.HasValue)
        {
            var currentCriteria = Criteria;
            Criteria = currentCriteria == null ? 
                l => l.AssignedTruck != null && l.AssignedTruck.MainDriverId == query.AssignedDriverId.Value : 
                l => currentCriteria.Compile()(l) && l.AssignedTruck != null && l.AssignedTruck.MainDriverId == query.AssignedDriverId.Value;
        }

        if (query.AssignedTruckId.HasValue)
        {
            var currentCriteria = Criteria;
            Criteria = currentCriteria == null ? 
                l => l.AssignedTruckId == query.AssignedTruckId.Value : 
                l => currentCriteria.Compile()(l) && l.AssignedTruckId == query.AssignedTruckId.Value;
        }

        if (query.CustomerId.HasValue)
        {
            var currentCriteria = Criteria;
            Criteria = currentCriteria == null ? 
                l => l.CustomerId == query.CustomerId.Value : 
                l => currentCriteria.Compile()(l) && l.CustomerId == query.CustomerId.Value;
        }
    }
}