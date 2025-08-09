using Logistics.Application.Queries.Reports.GetDriverReport;
using Logistics.Application.Queries.Reports.GetFinancialReport;
using Logistics.Application.Queries.Reports.GetLoadReport;
using Logistics.Application.Queries.Reports.GetLoadReportSummary;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Reports;
using Logistics.Shared.Identity.Policies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[Route("reports")]
[ApiController]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Load Reports

    [HttpGet("loads")]
    [ProducesResponseType(typeof(Result<PagedResult<LoadReportDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Reports.LoadReports)]
    public async Task<IActionResult> GetLoadReports([FromQuery] GetLoadReportQuery request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("loads/summary")]
    [ProducesResponseType(typeof(Result<LoadReportSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Reports.LoadReports)]
    public async Task<IActionResult> GetLoadReportSummary([FromQuery] GetLoadReportSummaryQuery request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Driver Reports

    [HttpGet("drivers")]
    [ProducesResponseType(typeof(Result<PagedResult<DriverReportDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Reports.DriverReports)]
    public async Task<IActionResult> GetDriverReports([FromQuery] GetDriverReportQuery request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("drivers/{driverId:guid}")]
    [ProducesResponseType(typeof(Result<DriverReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Reports.DriverReports)]
    public async Task<IActionResult> GetDriverReport(Guid driverId, [FromQuery] GetDriverReportQuery request)
    {
        request.DriverId = driverId;
        request.Size = 1; // Only get one driver
        
        var result = await _mediator.Send(request);
        if (!result.Success) return BadRequest(result);
        
        var driver = result.Data?.Items?.FirstOrDefault();
        if (driver == null)
        {
            return NotFound(Result.Failure($"Driver with ID {driverId} not found"));
        }

        return Ok(Result<DriverReportDto>.Success(driver));
    }

    [HttpGet("drivers/summary")]
    [ProducesResponseType(typeof(Result<DriverReportSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Reports.DriverReports)]
    public async Task<IActionResult> GetDriverReportSummary([FromQuery] GetDriverReportSummaryQuery request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Financial Reports

    [HttpGet("financial")]
    [ProducesResponseType(typeof(Result<FinancialReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Reports.FinancialReports)]
    public async Task<IActionResult> GetFinancialReport([FromQuery] GetFinancialReportQuery request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("financial/summary")]
    [ProducesResponseType(typeof(Result<FinancialSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Reports.FinancialReports)]
    public async Task<IActionResult> GetFinancialSummary([FromQuery] GetFinancialSummaryQuery request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("financial/cash-flow")]
    [ProducesResponseType(typeof(Result<CashFlowReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Reports.FinancialReports)]
    public async Task<IActionResult> GetCashFlowReport([FromQuery] GetCashFlowReportQuery request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Combined Reports

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(Result<DashboardReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Reports.View)]
    public async Task<IActionResult> GetDashboardReport([FromQuery] DashboardReportQuery request)
    {
        try
        {
            // Get summaries for dashboard
            var loadSummaryTask = _mediator.Send(new GetLoadReportSummaryQuery 
            { 
                StartDate = request.StartDate, 
                EndDate = request.EndDate 
            });
            
            var driverSummaryTask = _mediator.Send(new GetDriverReportSummaryQuery 
            { 
                StartDate = request.StartDate, 
                EndDate = request.EndDate 
            });
            
            var financialSummaryTask = _mediator.Send(new GetFinancialSummaryQuery 
            { 
                StartDate = request.StartDate, 
                EndDate = request.EndDate 
            });

            await Task.WhenAll(loadSummaryTask, driverSummaryTask, financialSummaryTask);

            var dashboard = new DashboardReportDto
            {
                ReportDate = DateTime.UtcNow,
                PeriodStart = request.StartDate ?? DateTime.UtcNow.AddMonths(-1),
                PeriodEnd = request.EndDate ?? DateTime.UtcNow,
                LoadSummary = loadSummaryTask.Result.Success ? loadSummaryTask.Result.Data : new LoadReportSummaryDto(),
                DriverSummary = driverSummaryTask.Result.Success ? driverSummaryTask.Result.Data : new DriverReportSummaryDto(),
                FinancialSummary = financialSummaryTask.Result.Success ? financialSummaryTask.Result.Data : new FinancialSummaryDto()
            };

            return Ok(Result<DashboardReportDto>.Success(dashboard));
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Failure($"Error generating dashboard report: {ex.Message}"));
        }
    }

    #endregion
}

// Additional DTOs for dashboard and summaries
public class DashboardReportQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class DashboardReportDto
{
    public DateTime ReportDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public LoadReportSummaryDto LoadSummary { get; set; } = new();
    public DriverReportSummaryDto DriverSummary { get; set; } = new();
    public FinancialSummaryDto FinancialSummary { get; set; } = new();
}