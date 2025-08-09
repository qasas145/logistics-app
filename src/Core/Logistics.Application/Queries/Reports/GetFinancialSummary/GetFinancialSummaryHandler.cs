using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Reports;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries.Reports.GetFinancialSummary;

public sealed class GetFinancialSummaryHandler : IRequestHandler<GetFinancialSummaryQuery, Result<FinancialSummaryDto>>
{
    private readonly IRepository<Load> _loadRepository;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<PayrollInvoice> _payrollRepository;

    public GetFinancialSummaryHandler(
        IRepository<Load> loadRepository,
        IRepository<Invoice> invoiceRepository,
        IRepository<PayrollInvoice> payrollRepository)
    {
        _loadRepository = loadRepository;
        _invoiceRepository = invoiceRepository;
        _payrollRepository = payrollRepository;
    }

    public async Task<Result<FinancialSummaryDto>> Handle(GetFinancialSummaryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-12);
            var endDate = request.EndDate ?? DateTime.UtcNow;

            // Get loads for revenue calculation
            var loads = await _loadRepository.GetAll()
                .Where(l => l.DispatchedDate >= startDate && l.DispatchedDate <= endDate)
                .ToListAsync(cancellationToken);

            // Get invoices for payment analysis
            var invoices = await _invoiceRepository.GetAll()
                .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate)
                .Include(i => i.Payments)
                .ToListAsync(cancellationToken);

            // Get payroll for expenses
            var payrollInvoices = await _payrollRepository.GetAll()
                .Where(p => p.PeriodStart >= startDate && p.PeriodEnd <= endDate)
                .ToListAsync(cancellationToken);

            var summary = new FinancialSummaryDto
            {
                TotalRevenue = loads.Sum(l => l.DeliveryCost.Amount),
                TotalExpenses = loads.Sum(l => l.CalcDriverShare()) + payrollInvoices.Sum(p => p.Total.Amount),
                TotalInvoices = invoices.Count,
                PaidInvoices = invoices.Count(i => i.Status == InvoiceStatus.Paid),
                OverdueInvoices = invoices.Count(i => i.Status == InvoiceStatus.Overdue),
                OutstandingBalance = invoices.Where(i => i.Status != InvoiceStatus.Paid).Sum(i => i.Total.Amount)
            };

            summary.GrossProfit = summary.TotalRevenue - summary.TotalExpenses;
            summary.NetProfit = summary.GrossProfit; // Simplified
            summary.ProfitMargin = summary.TotalRevenue > 0 ? (summary.GrossProfit / summary.TotalRevenue) * 100 : 0;
            summary.CollectionRate = summary.TotalInvoices > 0 ? (decimal)summary.PaidInvoices / summary.TotalInvoices * 100 : 0;

            // Calculate monthly trends
            var monthlyData = loads
                .GroupBy(l => new { l.DispatchedDate.Year, l.DispatchedDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(l => l.DeliveryCost.Amount),
                    Expenses = g.Sum(l => l.CalcDriverShare()),
                    LoadsCompleted = g.Count(l => l.Status == LoadStatus.Delivered)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            summary.MonthlyTrends = monthlyData.Select(m => new MonthlyFinancialTrendDto
            {
                Month = $"{m.Year}-{m.Month:D2}",
                Revenue = m.Revenue,
                Expenses = m.Expenses,
                Profit = m.Revenue - m.Expenses,
                LoadsCompleted = m.LoadsCompleted
            }).ToList();

            return Result<FinancialSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            return Result<FinancialSummaryDto>.Failure($"Error generating financial summary: {ex.Message}");
        }
    }
}