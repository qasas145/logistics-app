using Logistics.Application.Queries.Reports.GetLoadReport;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Specifications;
using System.Linq.Expressions;

namespace Logistics.Application.Specifications.Reports;

public sealed class LoadReportSpecification : BaseSpecification<Load>
{
    public LoadReportSpecification(GetLoadReportQuery query)
    {
        // Build the criteria expression step by step
        Expression<Func<Load, bool>>? criteria = null;

        if (query.StartDate.HasValue)
        {
            criteria = l => l.DispatchedDate >= query.StartDate.Value;
        }

        if (query.EndDate.HasValue)
        {
            var endCriteria = (Expression<Func<Load, bool>>)(l => l.DispatchedDate <= query.EndDate.Value);
            criteria = criteria == null ? endCriteria : CombineExpressions(criteria, endCriteria);
        }

        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<LoadStatus>(query.Status, true, out var status))
        {
            var statusCriteria = (Expression<Func<Load, bool>>)(l => l.Status == status);
            criteria = criteria == null ? statusCriteria : CombineExpressions(criteria, statusCriteria);
        }

        if (!string.IsNullOrWhiteSpace(query.LoadType) && Enum.TryParse<LoadType>(query.LoadType, true, out var loadType))
        {
            var typeCriteria = (Expression<Func<Load, bool>>)(l => l.Type == loadType);
            criteria = criteria == null ? typeCriteria : CombineExpressions(criteria, typeCriteria);
        }

        if (query.AssignedDriverId.HasValue)
        {
            var driverCriteria = (Expression<Func<Load, bool>>)(l => l.AssignedTruck != null && 
                l.AssignedTruck.MainDriverId == query.AssignedDriverId.Value);
            criteria = criteria == null ? driverCriteria : CombineExpressions(criteria, driverCriteria);
        }

        if (query.AssignedTruckId.HasValue)
        {
            var truckCriteria = (Expression<Func<Load, bool>>)(l => l.AssignedTruckId == query.AssignedTruckId.Value);
            criteria = criteria == null ? truckCriteria : CombineExpressions(criteria, truckCriteria);
        }

        if (query.CustomerId.HasValue)
        {
            var customerCriteria = (Expression<Func<Load, bool>>)(l => l.CustomerId == query.CustomerId.Value);
            criteria = criteria == null ? customerCriteria : CombineExpressions(criteria, customerCriteria);
        }

        if (query.MinDeliveryCost.HasValue)
        {
            var minCostCriteria = (Expression<Func<Load, bool>>)(l => l.DeliveryCost.Amount >= query.MinDeliveryCost.Value);
            criteria = criteria == null ? minCostCriteria : CombineExpressions(criteria, minCostCriteria);
        }

        if (query.MaxDeliveryCost.HasValue)
        {
            var maxCostCriteria = (Expression<Func<Load, bool>>)(l => l.DeliveryCost.Amount <= query.MaxDeliveryCost.Value);
            criteria = criteria == null ? maxCostCriteria : CombineExpressions(criteria, maxCostCriteria);
        }

        if (criteria != null)
        {
            Criteria = criteria;
        }
    }

    private static Expression<Func<Load, bool>> CombineExpressions(
        Expression<Func<Load, bool>> first,
        Expression<Func<Load, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(Load), "l");
        var firstBody = ReplaceParameter(first.Body, first.Parameters[0], parameter);
        var secondBody = ReplaceParameter(second.Body, second.Parameters[0], parameter);
        var combined = Expression.AndAlso(firstBody, secondBody);
        return Expression.Lambda<Func<Load, bool>>(combined, parameter);
    }

    private static Expression ReplaceParameter(Expression expression, ParameterExpression oldParameter, ParameterExpression newParameter)
    {
        return new ParameterReplacer(oldParameter, newParameter).Visit(expression);
    }
}

internal class ParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _oldParameter;
    private readonly ParameterExpression _newParameter;

    public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
    {
        _oldParameter = oldParameter;
        _newParameter = newParameter;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _oldParameter ? _newParameter : base.VisitParameter(node);
    }
}