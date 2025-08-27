using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class FinancialsReportHandler(ITenantUnitOfWork tenantUow) : IAppRequestHandler<FinancialsReportQuery, Result<FinancialsReportDto>>
{
    public async Task<Result<FinancialsReportDto>> Handle(FinancialsReportQuery req, CancellationToken ct)
    {
        var invoices = tenantUow.Repository<LoadInvoice>().Query();

        if (req.From is not null)
        {
            var from = req.From.Value;
            invoices = invoices.Where(i => i.CreatedAt >= from);
        }
        if (req.To is not null)
        {
            var to = req.To.Value;
            invoices = invoices.Where(i => i.CreatedAt <= to);
        }
        if (req.Status is not null)
        {
            invoices = invoices.Where(i => i.Status == req.Status);
        }
        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var term = req.Search.ToLower();
            invoices = invoices.Where(i => i.Customer != null && i.Customer.Name.ToLower().Contains(term));
        }

        var totalCount = invoices.Count();
        var totalInvoiced = invoices.Sum(i => i.Total);
        var totalPaid = invoices.Sum(i => i.Payments.Sum(p => p.Amount));
        var totalDue = totalInvoiced - totalPaid;

        var items = invoices
            .OrderByDescending(i => i.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(i => new FinancialsReportItemDto
            {
                InvoiceId = i.Id,
                InvoiceNumber = i.Number,
                Status = i.Status,
                Total = i.Total,
                Paid = i.Payments.Sum(p => p.Amount),
                DueDate = i.DueDate,
                CustomerName = i.Customer != null ? i.Customer.Name : null
            })
            .ToList();

        var dto = new FinancialsReportDto
        {
            Items = items,
            TotalCount = totalCount,
            TotalInvoiced = totalInvoiced,
            TotalPaid = totalPaid,
            TotalDue = totalDue
        };

        return Result<FinancialsReportDto>.Ok(dto);
    }
}

