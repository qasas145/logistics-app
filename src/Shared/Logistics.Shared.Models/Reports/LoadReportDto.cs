namespace Logistics.Shared.Models.Reports;

public class LoadReportDto
{
    public Guid Id { get; set; }
    public long Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    // Origin and Destination
    public string OriginAddress { get; set; } = string.Empty;
    public string DestinationAddress { get; set; } = string.Empty;
    public double Distance { get; set; }
    
    // Financial Information
    public decimal DeliveryCost { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal DriverShare { get; set; }
    public decimal CompanyRevenue { get; set; }
    
    // Dates
    public DateTime DispatchedDate { get; set; }
    public DateTime? PickUpDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public int? DeliveryTimeInHours { get; set; }
    
    // Assignment Information
    public string? AssignedTruckNumber { get; set; }
    public string? AssignedDriverName { get; set; }
    public string? AssignedDispatcherName { get; set; }
    public string? CustomerName { get; set; }
    
    // Invoice Information
    public bool HasInvoice { get; set; }
    public string? InvoiceStatus { get; set; }
    public decimal? InvoiceTotal { get; set; }
    public DateTime? InvoiceDueDate { get; set; }
    public bool IsPaid { get; set; }
}