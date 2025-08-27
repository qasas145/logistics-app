using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface IReportsApi
{
    Task<Result<LoadsReportDto>> GetLoadsReportAsync(LoadsReportQuery query);
    Task<Result<DriversReportDto>> GetDriversReportAsync(DriversReportQuery query);
    Task<Result<FinancialsReportDto>> GetFinancialsReportAsync(FinancialsReportQuery query);
}

