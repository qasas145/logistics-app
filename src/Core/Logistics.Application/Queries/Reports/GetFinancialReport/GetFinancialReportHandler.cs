using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Reports;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries.Reports.GetFinancialReport;

public sealed class GetFinancialReportHandler : IRequestHandler<GetFinancialReportQuery, Result<FinancialReportDto>>
{
    private readonly IRepository<Load> _loadRepository;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IRepository<PayrollInvoice> _payrollRepository;
    private readonly IRepository<Payment> _paymentRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<Customer> _customerRepository;

    public GetFinancialReportHandler(
        IRepository<Load> loadRepository,
        IRepository<Invoice> invoiceRepository,
        IRepository<PayrollInvoice> payrollRepository,
        IRepository<Payment> paymentRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Customer> customerRepository)
    {
        _loadRepository = loadRepository;
        _invoiceRepository = invoiceRepository;
        _payrollRepository = payrollRepository;
        _paymentRepository = paymentRepository;
        _employeeRepository = employeeRepository;
        _customerRepository = customerRepository;
    }

    public async Task<Result<FinancialReportDto>> Handle(GetFinancialReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get loads for the period
            var loads = await _loadRepository.GetAll()
                .Where(l => l.DispatchedDate >= request.StartDate && l.DispatchedDate <= request.EndDate)
                .Include(l => l.AssignedTruck)
                .Include(l => l.Customer)
                .Include(l => l.Invoices)
                .ThenInclude(i => i.Payments)
                .ToListAsync(cancellationToken);

            // Get all invoices for the period
            var allInvoices = await _invoiceRepository.GetAll()
                .Where(i => i.CreatedAt >= request.StartDate && i.CreatedAt <= request.EndDate)
                .Include(i => i.Payments)
                .ToListAsync(cancellationToken);

            // Get payroll invoices for expenses calculation
            var payrollInvoices = await _payrollRepository.GetAll()
                .Where(p => p.PeriodStart >= request.StartDate && p.PeriodEnd <= request.EndDate)
                .Include(p => p.Employee)
                .Include(p => p.Payments)
                .ToListAsync(cancellationToken);

            // Build the financial report
            var report = new FinancialReportDto
            {
                ReportDate = DateTime.UtcNow,
                ReportPeriod = request.Period,
                PeriodStart = request.StartDate,
                PeriodEnd = request.EndDate
            };

            // Calculate revenue breakdown
            report.Revenue = CalculateRevenueBreakdown(loads);

            // Calculate expense breakdown
            report.Expenses = CalculateExpenseBreakdown(loads, payrollInvoices);

            // Calculate profitability
            report.GrossProfit = report.Revenue.TotalRevenue - report.Expenses.TotalExpenses;
            report.NetProfit = report.GrossProfit; // Simplified - in real scenario, consider taxes, etc.
            report.ProfitMargin = report.Revenue.TotalRevenue > 0 ? 
                (report.GrossProfit / report.Revenue.TotalRevenue) * 100 : 0;

            // Calculate payment status
            report.PaymentStatus = CalculatePaymentStatus(allInvoices);

            // Calculate invoice summary
            report.InvoiceSummary = CalculateInvoiceSummary(allInvoices);

            // Get top performers if requested
            if (request.IncludeTopPerformers)
            {
                report.TopDrivers = await CalculateTopDrivers(loads, request.TopPerformersLimit, cancellationToken);
                report.TopCustomers = await CalculateTopCustomers(loads, request.TopPerformersLimit, cancellationToken);
            }

            // Calculate comparison with previous period if requested
            if (request.IncludeComparison)
            {
                report.Comparison = await CalculateComparison(request, cancellationToken);
            }

            return Result<FinancialReportDto>.Succeed(report);
        }
        catch (Exception ex)
        {
            return Result<FinancialReportDto>.Fail($"Error generating financial report: {ex.Message}");
        }
    }

    private static RevenueBreakdownDto CalculateRevenueBreakdown(List<Load> loads)
    {
        var deliveredLoads = loads.Where(l => l.Status == LoadStatus.Delivered).ToList();
        
        return new RevenueBreakdownDto
        {
            TotalRevenue = loads.Sum(l => l.DeliveryCost.Amount),
            LoadRevenue = loads.Sum(l => l.DeliveryCost.Amount),
            FuelSurcharges = 0, // Would need additional data structure for this
            AccessorialCharges = 0, // Would need additional data structure for this
            TotalLoadsDelivered = deliveredLoads.Count,
            AverageRevenuePerLoad = loads.Count > 0 ? loads.Average(l => l.DeliveryCost.Amount) : 0,
            AverageRevenuePerMile = loads.Sum(l => l.Distance) > 0 ? 
                loads.Sum(l => l.DeliveryCost.Amount) / (decimal)loads.Sum(l => l.Distance) : 0,
            RevenueByLoadType = loads.GroupBy(l => l.Type)
                .Select(g => new RevenueByTypeDto
                {
                    LoadType = g.Key.ToString(),
                    Revenue = g.Sum(l => l.DeliveryCost.Amount),
                    LoadCount = g.Count(),
                    Percentage = loads.Sum(l => l.DeliveryCost.Amount) > 0 ? 
                        (g.Sum(l => l.DeliveryCost.Amount) / loads.Sum(l => l.DeliveryCost.Amount)) * 100 : 0
                }).ToList()
        };
    }

    private static ExpenseBreakdownDto CalculateExpenseBreakdown(List<Load> loads, List<PayrollInvoice> payrollInvoices)
    {
        var driverPayouts = loads.Sum(l => l.CalcDriverShare());
        var payrollExpenses = payrollInvoices.Sum(p => p.Total.Amount);
        
        return new ExpenseBreakdownDto
        {
            DriverPayouts = driverPayouts,
            PayrollExpenses = payrollExpenses,
            TotalExpenses = driverPayouts + payrollExpenses,
            FuelCosts = 0, // Would need additional data structure
            MaintenanceCosts = 0, // Would need additional data structure
            InsuranceCosts = 0, // Would need additional data structure
            OperationalExpenses = 0, // Would need additional data structure
            ExpensesByCategory = new List<ExpenseByTypeDto>
            {
                new() { Category = "Driver Payouts", Amount = driverPayouts, Percentage = 0 },
                new() { Category = "Payroll", Amount = payrollExpenses, Percentage = 0 }
            }
        };
    }

    private static PaymentStatusDto CalculatePaymentStatus(List<Invoice> invoices)
    {
        var totalReceivables = invoices.Sum(i => i.Total.Amount);
        var paidAmount = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.Total.Amount);
        var pendingAmount = invoices.Where(i => i.Status == InvoiceStatus.Issued).Sum(i => i.Total.Amount);
        var overdueAmount = invoices.Where(i => i.Status == InvoiceStatus.Overdue).Sum(i => i.Total.Amount);

        return new PaymentStatusDto
        {
            TotalReceivables = totalReceivables,
            PaidAmount = paidAmount,
            PendingAmount = pendingAmount,
            OverdueAmount = overdueAmount,
            TotalInvoices = invoices.Count,
            PaidInvoices = invoices.Count(i => i.Status == InvoiceStatus.Paid),
            PendingInvoices = invoices.Count(i => i.Status == InvoiceStatus.Issued),
            OverdueInvoices = invoices.Count(i => i.Status == InvoiceStatus.Overdue),
            CollectionPercentage = totalReceivables > 0 ? (paidAmount / totalReceivables) * 100 : 0,
            AverageDaysToPayment = CalculateAverageDaysToPayment(invoices)
        };
    }

    private static InvoiceSummaryDto CalculateInvoiceSummary(List<Invoice> invoices)
    {
        return new InvoiceSummaryDto
        {
            TotalInvoicesIssued = invoices.Count,
            TotalInvoiceValue = invoices.Sum(i => i.Total.Amount),
            AverageInvoiceValue = invoices.Count > 0 ? invoices.Average(i => i.Total.Amount) : 0,
            InvoicesPaid = invoices.Count(i => i.Status == InvoiceStatus.Paid),
            InvoicesPending = invoices.Count(i => i.Status == InvoiceStatus.Issued),
            InvoicesOverdue = invoices.Count(i => i.Status == InvoiceStatus.Overdue),
            InvoiceAging = CalculateInvoiceAging(invoices)
        };
    }

    private async Task<List<TopPerformerDto>> CalculateTopDrivers(List<Load> loads, int limit, CancellationToken cancellationToken)
    {
        var driverPerformance = loads
            .Where(l => l.AssignedTruck?.MainDriverId != null)
            .GroupBy(l => l.AssignedTruck!.MainDriverId!.Value)
            .Select(g => new
            {
                DriverId = g.Key,
                TotalEarnings = g.Sum(l => l.CalcDriverShare()),
                LoadsCompleted = g.Count(l => l.Status == LoadStatus.Delivered),
                TotalDistance = g.Sum(l => l.Distance)
            })
            .OrderByDescending(x => x.TotalEarnings)
            .Take(limit)
            .ToList();

        var result = new List<TopPerformerDto>();
        
        foreach (var perf in driverPerformance)
        {
            var driver = await _employeeRepository.GetByIdAsync(perf.DriverId);
            if (driver != null)
            {
                result.Add(new TopPerformerDto
                {
                    Id = perf.DriverId,
                    Name = driver.GetFullName(),
                    TotalEarnings = perf.TotalEarnings,
                    LoadsCompleted = perf.LoadsCompleted,
                    TotalDistance = perf.TotalDistance,
                    AveragePerLoad = perf.LoadsCompleted > 0 ? perf.TotalEarnings / perf.LoadsCompleted : 0
                });
            }
        }

        return result;
    }

    private async Task<List<TopCustomerDto>> CalculateTopCustomers(List<Load> loads, int limit, CancellationToken cancellationToken)
    {
        var customerPerformance = loads
            .Where(l => l.CustomerId != null)
            .GroupBy(l => l.CustomerId!.Value)
            .Select(g => new
            {
                CustomerId = g.Key,
                TotalRevenue = g.Sum(l => l.DeliveryCost.Amount),
                TotalLoads = g.Count(),
                OutstandingBalance = g.SelectMany(l => l.Invoices)
                    .Where(i => i.Status != InvoiceStatus.Paid)
                    .Sum(i => i.Total.Amount)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(limit)
            .ToList();

        var result = new List<TopCustomerDto>();
        
        foreach (var perf in customerPerformance)
        {
            var customer = await _customerRepository.GetByIdAsync(perf.CustomerId);
            if (customer != null)
            {
                result.Add(new TopCustomerDto
                {
                    Id = perf.CustomerId,
                    Name = customer.Name,
                    TotalRevenue = perf.TotalRevenue,
                    TotalLoads = perf.TotalLoads,
                    AverageLoadValue = perf.TotalLoads > 0 ? perf.TotalRevenue / perf.TotalLoads : 0,
                    OutstandingBalance = perf.OutstandingBalance
                });
            }
        }

        return result;
    }

    private async Task<FinancialComparisonDto> CalculateComparison(GetFinancialReportQuery request, CancellationToken cancellationToken)
    {
        var periodDuration = request.EndDate - request.StartDate;
        var previousStart = request.StartDate - periodDuration;
        var previousEnd = request.StartDate.AddDays(-1);

        var previousLoads = await _loadRepository.GetAll()
            .Where(l => l.DispatchedDate >= previousStart && l.DispatchedDate <= previousEnd)
            .ToListAsync(cancellationToken);

        var currentRevenue = await _loadRepository.GetAll()
            .Where(l => l.DispatchedDate >= request.StartDate && l.DispatchedDate <= request.EndDate)
            .SumAsync(l => l.DeliveryCost.Amount, cancellationToken);

        var previousRevenue = previousLoads.Sum(l => l.DeliveryCost.Amount);
        var currentLoads = await _loadRepository.GetAll()
            .Where(l => l.DispatchedDate >= request.StartDate && l.DispatchedDate <= request.EndDate)
            .CountAsync(cancellationToken);

        return new FinancialComparisonDto
        {
            PreviousPeriodRevenue = previousRevenue,
            RevenueGrowth = currentRevenue - previousRevenue,
            RevenueGrowthPercentage = previousRevenue > 0 ? ((currentRevenue - previousRevenue) / previousRevenue) * 100 : 0,
            PreviousPeriodLoads = previousLoads.Count,
            LoadGrowth = currentLoads - previousLoads.Count,
            LoadGrowthPercentage = previousLoads.Count > 0 ? ((decimal)(currentLoads - previousLoads.Count) / previousLoads.Count) * 100 : 0
        };
    }

    private static double CalculateAverageDaysToPayment(List<Invoice> invoices)
    {
        var paidInvoices = invoices.Where(i => i.Status == InvoiceStatus.Paid && i.Payments.Any()).ToList();
        if (!paidInvoices.Any()) return 0;

        var paymentDays = paidInvoices.Select(i =>
            i.Payments.Max(p => p.CreatedAt).Subtract(i.CreatedAt).TotalDays).ToList();

        return paymentDays.Average();
    }

    private static List<InvoiceAgingDto> CalculateInvoiceAging(List<Invoice> invoices)
    {
        var unpaidInvoices = invoices.Where(i => i.Status != InvoiceStatus.Paid).ToList();
        var now = DateTime.UtcNow;

        var aging = new List<InvoiceAgingDto>();
        var totalAmount = unpaidInvoices.Sum(i => i.Total.Amount);

        // 0-30 days
        var current = unpaidInvoices.Where(i => (now - i.CreatedAt).TotalDays <= 30).ToList();
        aging.Add(new InvoiceAgingDto
        {
            AgeRange = "0-30 days",
            Count = current.Count,
            Amount = current.Sum(i => i.Total.Amount),
            Percentage = totalAmount > 0 ? (current.Sum(i => i.Total.Amount) / totalAmount) * 100 : 0
        });

        // 31-60 days
        var thirtyToSixty = unpaidInvoices.Where(i => (now - i.CreatedAt).TotalDays > 30 && (now - i.CreatedAt).TotalDays <= 60).ToList();
        aging.Add(new InvoiceAgingDto
        {
            AgeRange = "31-60 days",
            Count = thirtyToSixty.Count,
            Amount = thirtyToSixty.Sum(i => i.Total.Amount),
            Percentage = totalAmount > 0 ? (thirtyToSixty.Sum(i => i.Total.Amount) / totalAmount) * 100 : 0
        });

        // 61-90 days
        var sixtyToNinety = unpaidInvoices.Where(i => (now - i.CreatedAt).TotalDays > 60 && (now - i.CreatedAt).TotalDays <= 90).ToList();
        aging.Add(new InvoiceAgingDto
        {
            AgeRange = "61-90 days",
            Count = sixtyToNinety.Count,
            Amount = sixtyToNinety.Sum(i => i.Total.Amount),
            Percentage = totalAmount > 0 ? (sixtyToNinety.Sum(i => i.Total.Amount) / totalAmount) * 100 : 0
        });

        // 90+ days
        var overNinety = unpaidInvoices.Where(i => (now - i.CreatedAt).TotalDays > 90).ToList();
        aging.Add(new InvoiceAgingDto
        {
            AgeRange = "90+ days",
            Count = overNinety.Count,
            Amount = overNinety.Sum(i => i.Total.Amount),
            Percentage = totalAmount > 0 ? (overNinety.Sum(i => i.Total.Amount) / totalAmount) * 100 : 0
        });

        return aging;
    }
}