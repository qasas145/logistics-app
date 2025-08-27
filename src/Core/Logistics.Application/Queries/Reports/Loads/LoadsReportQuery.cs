using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class LoadsReportQuery : Logistics.Shared.Models.LoadsReportQuery, IAppRequest<Result<LoadsReportDto>>
{
}

