using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models.Reports;

namespace Logistics.Mappings;

public static class LoadReportMapper
{
    public static LoadReportDto MapToLoadReportDto(Load load, bool includeInvoiceDetails = true)
    {
        var dto = new LoadReportDto
        {
            Id = load.Id,
            Number = load.Number,
            Name = load.Name,
            Type = load.Type.ToString(),
            Status = load.Status.ToString(),
            OriginAddress = load.OriginAddress.ToString(),
            DestinationAddress = load.DestinationAddress.ToString(),
            Distance = load.Distance,
            DeliveryCost = load.DeliveryCost.Amount,
            Currency = load.DeliveryCost.Currency,
            DriverShare = load.CalcDriverShare(),
            CompanyRevenue = load.DeliveryCost.Amount - load.CalcDriverShare(),
            DispatchedDate = load.DispatchedDate,
            PickUpDate = load.PickUpDate,
            DeliveryDate = load.DeliveryDate,
            AssignedTruckNumber = load.AssignedTruck?.Number,
            AssignedDriverName = load.AssignedTruck?.GetDriversNames(),
            AssignedDispatcherName = load.AssignedDispatcher?.GetFullName(),
            CustomerName = load.Customer?.Name
        };

        // Calculate delivery time in hours
        if (load.PickUpDate.HasValue && load.DeliveryDate.HasValue)
        {
            dto.DeliveryTimeInHours = (int)load.DeliveryDate.Value.Subtract(load.PickUpDate.Value).TotalHours;
        }

        // Include invoice details if requested
        if (includeInvoiceDetails && load.Invoices.Any())
        {
            var invoice = load.Invoices.First(); // Assuming one invoice per load
            dto.HasInvoice = true;
            dto.InvoiceStatus = invoice.Status.ToString();
            dto.InvoiceTotal = invoice.Total.Amount;
            dto.InvoiceDueDate = invoice.DueDate;
            dto.IsPaid = invoice.Status == InvoiceStatus.Paid;
        }
        else
        {
            dto.HasInvoice = false;
            dto.IsPaid = false;
        }

        return dto;
    }

    public static List<LoadReportDto> MapToLoadReportDtos(IEnumerable<Load> loads, bool includeInvoiceDetails = true)
    {
        return loads.Select(load => MapToLoadReportDto(load, includeInvoiceDetails)).ToList();
    }
}