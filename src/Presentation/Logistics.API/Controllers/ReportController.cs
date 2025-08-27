using Logistics.Application.Queries;
using Logistics.Application.Services.Reporting;
using Logistics.Shared.Identity.Policies;
using Logistics.Shared.Models;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[ApiController]
[Route("reports")]
public class ReportController(IMediator mediator, IReportExportService exportService) : ControllerBase
{
    [HttpGet("loads")]
    [ProducesResponseType(typeof(Result<LoadsReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetLoadsReport([FromQuery] LoadsReportQuery request)
    {
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("loads/export")]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> ExportLoads([FromQuery] LoadsReportQuery request, [FromQuery] string format = "csv")
    {
        var result = await mediator.Send(request);
        if (!result.Success) return BadRequest(result);
        var export = await exportService.ExportLoadsAsync(result.Data!, format);
        return File(export.Content, export.ContentType, export.FileName);
    }

    [HttpGet("drivers")]
    [ProducesResponseType(typeof(Result<DriversReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetDriversReport([FromQuery] DriversReportQuery request)
    {
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("drivers/export")]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> ExportDrivers([FromQuery] DriversReportQuery request, [FromQuery] string format = "csv")
    {
        var result = await mediator.Send(request);
        if (!result.Success) return BadRequest(result);
        var export = await exportService.ExportDriversAsync(result.Data!, format);
        return File(export.Content, export.ContentType, export.FileName);
    }

    [HttpGet("financials")]
    [ProducesResponseType(typeof(Result<FinancialsReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> GetFinancialsReport([FromQuery] FinancialsReportQuery request)
    {
        var result = await mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("financials/export")]
    [Authorize(Policy = Permissions.Stats.View)]
    public async Task<IActionResult> ExportFinancials([FromQuery] FinancialsReportQuery request, [FromQuery] string format = "csv")
    {
        var result = await mediator.Send(request);
        if (!result.Success) return BadRequest(result);
        var export = await exportService.ExportFinancialsAsync(result.Data!, format);
        return File(export.Content, export.ContentType, export.FileName);
    }
}

