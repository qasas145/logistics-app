using Logistics.Shared.Models;

namespace Logistics.Application.Services.Reporting;

public interface IReportExportService
{
    Task<(byte[] Content, string ContentType, string FileName)> ExportLoadsAsync(LoadsReportDto data, string format, CancellationToken ct = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportDriversAsync(DriversReportDto data, string format, CancellationToken ct = default);
    Task<(byte[] Content, string ContentType, string FileName)> ExportFinancialsAsync(FinancialsReportDto data, string format, CancellationToken ct = default);
}

