namespace Logistics.Shared.Models.Reports;

public class GetFinancialReportQuery
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string ReportType { get; set; } = "Summary"; // Summary, Detailed, Comparison
    public string Period { get; set; } = "Monthly"; // Daily, Weekly, Monthly, Quarterly, Yearly
    public bool IncludeComparison { get; set; } = true;
    public bool IncludeTopPerformers { get; set; } = true;
    public int TopPerformersLimit { get; set; } = 10;
    public bool IncludeInvoiceAging { get; set; } = true;
    public string? Currency { get; set; }
}

public class GetFinancialSummaryQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Period { get; set; } = "Monthly";
}

public class FinancialSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int OverdueInvoices { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal CollectionRate { get; set; }
    public List<MonthlyFinancialTrendDto> MonthlyTrends { get; set; } = new();
}

public class MonthlyFinancialTrendDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal Profit { get; set; }
    public int LoadsCompleted { get; set; }
}

public class GetCashFlowReportQuery
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Period { get; set; } = "Monthly"; // Daily, Weekly, Monthly
}

public class CashFlowReportDto
{
    public DateTime ReportDate { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal TotalInflows { get; set; }
    public decimal TotalOutflows { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal NetCashFlow { get; set; }
    public List<CashFlowItemDto> Inflows { get; set; } = new();
    public List<CashFlowItemDto> Outflows { get; set; } = new();
    public List<CashFlowPeriodDto> PeriodBreakdown { get; set; } = new();
}

public class CashFlowItemDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class CashFlowPeriodDto
{
    public DateTime Date { get; set; }
    public decimal Inflow { get; set; }
    public decimal Outflow { get; set; }
    public decimal NetFlow { get; set; }
    public decimal RunningBalance { get; set; }
}