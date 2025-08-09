using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Reports;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries.Reports.GetCashFlowReport;

public sealed class GetCashFlowReportHandler : IRequestHandler<GetCashFlowReportQuery, Result<CashFlowReportDto>>
{
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<PayrollInvoice> _payrollRepository;

    public GetCashFlowReportHandler(
        IRepository<Payment> paymentRepository,
        IRepository<Invoice> invoiceRepository,
        IRepository<PayrollInvoice> payrollRepository)
    {
        _paymentRepository = paymentRepository;
        _invoiceRepository = invoiceRepository;
        _payrollRepository = payrollRepository;
    }

    public async Task<Result<CashFlowReportDto>> Handle(GetCashFlowReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get payments received (inflows)
            var paymentsReceived = await _paymentRepository.GetAll()
                .Where(p => p.CreatedAt >= request.StartDate && p.CreatedAt <= request.EndDate && 
                           p.Status == PaymentStatus.Completed)
                .ToListAsync(cancellationToken);

            // Get payroll payments (outflows)
            var payrollPayments = await _payrollRepository.GetAll()
                .Where(p => p.PeriodStart >= request.StartDate && p.PeriodEnd <= request.EndDate &&
                           p.Status == InvoiceStatus.Paid)
                .Include(p => p.Payments)
                .ToListAsync(cancellationToken);

            var report = new CashFlowReportDto
            {
                ReportDate = DateTime.UtcNow,
                Period = request.Period,
                TotalInflows = paymentsReceived.Sum(p => p.Amount.Amount),
                TotalOutflows = payrollPayments.Sum(p => p.Total.Amount),
                OpeningBalance = 0, // Would need to track this separately
                ClosingBalance = 0  // Would need to track this separately
            };

            report.NetCashFlow = report.TotalInflows - report.TotalOutflows;
            report.ClosingBalance = report.OpeningBalance + report.NetCashFlow;

            // Calculate inflows breakdown
            report.Inflows = new List<CashFlowItemDto>
            {
                new()
                {
                    Category = "Customer Payments",
                    Amount = paymentsReceived.Sum(p => p.Amount.Amount),
                    Percentage = report.TotalInflows > 0 ? 
                        (paymentsReceived.Sum(p => p.Amount.Amount) / report.TotalInflows) * 100 : 0,
                    Description = "Payments received from customers for completed loads"
                }
            };

            // Calculate outflows breakdown
            report.Outflows = new List<CashFlowItemDto>
            {
                new()
                {
                    Category = "Driver Payroll",
                    Amount = payrollPayments.Sum(p => p.Total.Amount),
                    Percentage = report.TotalOutflows > 0 ? 
                        (payrollPayments.Sum(p => p.Total.Amount) / report.TotalOutflows) * 100 : 0,
                    Description = "Payroll payments to drivers and employees"
                }
            };

            // Calculate period breakdown based on the requested period
            report.PeriodBreakdown = CalculatePeriodBreakdown(
                paymentsReceived, 
                payrollPayments, 
                request.StartDate, 
                request.EndDate, 
                request.Period);

            return Result<CashFlowReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            return Result<CashFlowReportDto>.Failure($"Error generating cash flow report: {ex.Message}");
        }
    }

    private static List<CashFlowPeriodDto> CalculatePeriodBreakdown(
        List<Payment> inflows,
        List<PayrollInvoice> outflows,
        DateTime startDate,
        DateTime endDate,
        string period)
    {
        var breakdown = new List<CashFlowPeriodDto>();
        var currentDate = startDate;
        var runningBalance = 0m; // Would need to get actual opening balance

        while (currentDate <= endDate)
        {
            var periodEnd = period.ToLower() switch
            {
                "daily" => currentDate.Date.AddDays(1).AddTicks(-1),
                "weekly" => currentDate.Date.AddDays(7).AddTicks(-1),
                "monthly" => currentDate.Date.AddMonths(1).AddTicks(-1),
                _ => currentDate.Date.AddMonths(1).AddTicks(-1)
            };

            if (periodEnd > endDate)
                periodEnd = endDate;

            var periodInflows = inflows
                .Where(p => p.CreatedAt >= currentDate && p.CreatedAt <= periodEnd)
                .Sum(p => p.Amount.Amount);

            var periodOutflows = outflows
                .Where(p => p.CreatedAt >= currentDate && p.CreatedAt <= periodEnd)
                .Sum(p => p.Total.Amount);

            var netFlow = periodInflows - periodOutflows;
            runningBalance += netFlow;

            breakdown.Add(new CashFlowPeriodDto
            {
                Date = currentDate,
                Inflow = periodInflows,
                Outflow = periodOutflows,
                NetFlow = netFlow,
                RunningBalance = runningBalance
            });

            currentDate = period.ToLower() switch
            {
                "daily" => currentDate.AddDays(1),
                "weekly" => currentDate.AddDays(7),
                "monthly" => currentDate.AddMonths(1),
                _ => currentDate.AddMonths(1)
            };
        }

        return breakdown;
    }
}