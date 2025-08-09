namespace Logistics.Shared.Models.Reports;

public class FinancialReportDto
{
    public DateTime ReportDate { get; set; }
    public string ReportPeriod { get; set; } = string.Empty; // Daily, Weekly, Monthly, Yearly
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    
    // Revenue Breakdown
    public RevenueBreakdownDto Revenue { get; set; } = new();
    
    // Expenses Breakdown
    public ExpenseBreakdownDto Expenses { get; set; } = new();
    
    // Profitability
    public decimal GrossProfit { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    
    // Payment Status
    public PaymentStatusDto PaymentStatus { get; set; } = new();
    
    // Invoice Summary
    public InvoiceSummaryDto InvoiceSummary { get; set; } = new();
    
    // Top Performers
    public List<TopPerformerDto> TopDrivers { get; set; } = new();
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
    
    // Period Comparison
    public FinancialComparisonDto Comparison { get; set; } = new();
}

public class RevenueBreakdownDto
{
    public decimal TotalRevenue { get; set; }
    public decimal LoadRevenue { get; set; }
    public decimal FuelSurcharges { get; set; }
    public decimal AccessorialCharges { get; set; }
    public int TotalLoadsDelivered { get; set; }
    public decimal AverageRevenuePerLoad { get; set; }
    public decimal AverageRevenuePerMile { get; set; }
    public List<RevenueByTypeDto> RevenueByLoadType { get; set; } = new();
}

public class ExpenseBreakdownDto
{
    public decimal TotalExpenses { get; set; }
    public decimal DriverPayouts { get; set; }
    public decimal FuelCosts { get; set; }
    public decimal MaintenanceCosts { get; set; }
    public decimal InsuranceCosts { get; set; }
    public decimal OperationalExpenses { get; set; }
    public decimal PayrollExpenses { get; set; }
    public List<ExpenseByTypeDto> ExpensesByCategory { get; set; } = new();
}

public class PaymentStatusDto
{
    public decimal TotalReceivables { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal OverdueAmount { get; set; }
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int PendingInvoices { get; set; }
    public int OverdueInvoices { get; set; }
    public decimal CollectionPercentage { get; set; }
    public double AverageDaysToPayment { get; set; }
}

public class InvoiceSummaryDto
{
    public int TotalInvoicesIssued { get; set; }
    public decimal TotalInvoiceValue { get; set; }
    public decimal AverageInvoiceValue { get; set; }
    public int InvoicesPaid { get; set; }
    public int InvoicesPending { get; set; }
    public int InvoicesOverdue { get; set; }
    public List<InvoiceAgingDto> InvoiceAging { get; set; } = new();
}

public class TopPerformerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalEarnings { get; set; }
    public int LoadsCompleted { get; set; }
    public double TotalDistance { get; set; }
    public decimal AveragePerLoad { get; set; }
}

public class TopCustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int TotalLoads { get; set; }
    public decimal AverageLoadValue { get; set; }
    public decimal OutstandingBalance { get; set; }
}

public class FinancialComparisonDto
{
    public decimal PreviousPeriodRevenue { get; set; }
    public decimal RevenueGrowth { get; set; }
    public decimal RevenueGrowthPercentage { get; set; }
    
    public decimal PreviousPeriodProfit { get; set; }
    public decimal ProfitGrowth { get; set; }
    public decimal ProfitGrowthPercentage { get; set; }
    
    public int PreviousPeriodLoads { get; set; }
    public int LoadGrowth { get; set; }
    public decimal LoadGrowthPercentage { get; set; }
}

public class RevenueByTypeDto
{
    public string LoadType { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int LoadCount { get; set; }
    public decimal Percentage { get; set; }
}

public class ExpenseByTypeDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class InvoiceAgingDto
{
    public string AgeRange { get; set; } = string.Empty; // "0-30 days", "31-60 days", etc.
    public int Count { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}