using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class DriversReportQuery : Logistics.Shared.Models.DriversReportQuery, IAppRequest<Result<DriversReportDto>>
{
}

