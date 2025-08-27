using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class FinancialsReportQuery : Logistics.Shared.Models.FinancialsReportQuery, IAppRequest<Result<FinancialsReportDto>>
{
}

