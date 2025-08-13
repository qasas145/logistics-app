using Logistics.Application.Queries.Reports.GetDriverReport;
using Logistics.Application.Queries.Reports.GetFinancialReport;
using Logistics.Application.Queries.Reports.GetLoadReport;
using Logistics.Application.Queries.Reports.GetLoadReportSummary;
using Logistics.Application.Services.Reports;
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
    private readonly IReportExportService _exportService;

    public ReportsController(IMediator mediator, IReportExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
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
        request.PageSize = 1; // Only get one driver
        
        var result = await _mediator.Send(request);
        if (!result.Success) return BadRequest(result);
        
        var driver = result.Data?.Items?.FirstOrDefault();
        if (driver == null)
        {
            return NotFound(Result.Fail($"Driver with ID {driverId} not found"));
        }

        return Ok(Result<DriverReportDto>.Succeed(driver));
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

            return Ok(Result<DashboardReportDto>.Succeed(dashboard));
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Fail($"Error generating dashboard report: {ex.Message}"));
        }
    }

    #endregion

    #region Export Endpoints

    [HttpGet("loads/export/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [Authorize(Policy = Permissions.Reports.Export)]
    public async Task<IActionResult> ExportLoadReportToPdf([FromQuery] GetLoadReportQuery request)
    {
        try
        {
            var loadReportsResult = await _mediator.Send(request);
            if (!loadReportsResult.Success) return BadRequest(loadReportsResult);

            var summaryResult = await _mediator.Send(new GetLoadReportSummaryQuery 
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status,
                LoadType = request.LoadType
            });

            var pdfBytes = await _exportService.ExportLoadReportToPdfAsync(
                loadReportsResult.Data?.Items?.ToList() ?? new List<LoadReportDto>(),
                summaryResult.Success ? summaryResult.Data : null);

            return File(pdfBytes, "text/html", $"load-report-{DateTime.Now:yyyy-MM-dd}.html");
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Fail($"Error exporting load report to PDF: {ex.Message}"));
        }
    }

    [HttpGet("loads/export/excel")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [Authorize(Policy = Permissions.Reports.Export)]
    public async Task<IActionResult> ExportLoadReportToExcel([FromQuery] GetLoadReportQuery request)
    {
        try
        {
            var loadReportsResult = await _mediator.Send(request);
            if (!loadReportsResult.Success) return BadRequest(loadReportsResult);

            var summaryResult = await _mediator.Send(new GetLoadReportSummaryQuery 
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = request.Status,
                LoadType = request.LoadType
            });

            var excelBytes = await _exportService.ExportLoadReportToExcelAsync(
                loadReportsResult.Data?.Items?.ToList() ?? new List<LoadReportDto>(),
                summaryResult.Success ? summaryResult.Data : null);

            return File(excelBytes, "text/csv", $"load-report-{DateTime.Now:yyyy-MM-dd}.csv");
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Fail($"Error exporting load report to Excel: {ex.Message}"));
        }
    }

    [HttpGet("drivers/export/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [Authorize(Policy = Permissions.Reports.Export)]
    public async Task<IActionResult> ExportDriverReportToPdf([FromQuery] GetDriverReportQuery request)
    {
        try
        {
            var driverReportsResult = await _mediator.Send(request);
            if (!driverReportsResult.Success) return BadRequest(driverReportsResult);

            var pdfBytes = await _exportService.ExportDriverReportToPdfAsync(
                driverReportsResult.Data?.Items?.ToList() ?? new List<DriverReportDto>());

            return File(pdfBytes, "text/html", $"driver-report-{DateTime.Now:yyyy-MM-dd}.html");
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Fail($"Error exporting driver report to PDF: {ex.Message}"));
        }
    }

    [HttpGet("drivers/export/excel")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [Authorize(Policy = Permissions.Reports.Export)]
    public async Task<IActionResult> ExportDriverReportToExcel([FromQuery] GetDriverReportQuery request)
    {
        try
        {
            var driverReportsResult = await _mediator.Send(request);
            if (!driverReportsResult.Success) return BadRequest(driverReportsResult);

            var excelBytes = await _exportService.ExportDriverReportToExcelAsync(
                driverReportsResult.Data?.Items?.ToList() ?? new List<DriverReportDto>());

            return File(excelBytes, "text/csv", $"driver-report-{DateTime.Now:yyyy-MM-dd}.csv");
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Fail($"Error exporting driver report to Excel: {ex.Message}"));
        }
    }

    [HttpGet("financial/export/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [Authorize(Policy = Permissions.Reports.Export)]
    public async Task<IActionResult> ExportFinancialReportToPdf([FromQuery] GetFinancialReportQuery request)
    {
        try
        {
            var financialReportResult = await _mediator.Send(request);
            if (!financialReportResult.Success) return BadRequest(financialReportResult);

            var pdfBytes = await _exportService.ExportFinancialReportToPdfAsync(financialReportResult.Data!);

            return File(pdfBytes, "text/html", $"financial-report-{DateTime.Now:yyyy-MM-dd}.html");
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Fail($"Error exporting financial report to PDF: {ex.Message}"));
        }
    }

    [HttpGet("financial/export/excel")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [Authorize(Policy = Permissions.Reports.Export)]
    public async Task<IActionResult> ExportFinancialReportToExcel([FromQuery] GetFinancialReportQuery request)
    {
        try
        {
            var financialReportResult = await _mediator.Send(request);
            if (!financialReportResult.Success) return BadRequest(financialReportResult);

            var excelBytes = await _exportService.ExportFinancialReportToExcelAsync(financialReportResult.Data!);

            return File(excelBytes, "text/csv", $"financial-report-{DateTime.Now:yyyy-MM-dd}.csv");
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Fail($"Error exporting financial report to Excel: {ex.Message}"));
        }
    }

    [HttpGet("dashboard/export/pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [Authorize(Policy = Permissions.Reports.Export)]
    public async Task<IActionResult> ExportDashboardReportToPdf([FromQuery] DashboardReportQuery request)
    {
        try
        {
            var dashboardResult = await _mediator.Send(new GetLoadReportSummaryQuery 
            { 
                StartDate = request.StartDate, 
                EndDate = request.EndDate 
            });
            
            // For simplicity, creating a basic dashboard export
            // In a real implementation, you would create a proper dashboard DTO
            var dashboard = new DashboardReportDto
            {
                ReportDate = DateTime.UtcNow.ToString(),
                PeriodStart = request.StartDate?.ToString() ?? DateTime.UtcNow.AddMonths(-1).ToString(),
                PeriodEnd = request.EndDate?.ToString() ?? DateTime.UtcNow.ToString(),
                LoadSummary = dashboardResult.Success ? dashboardResult.Data! : new LoadReportSummaryDto(),
                DriverSummary = new DriverReportSummaryDto(),
                FinancialSummary = new FinancialSummaryDto()
            };

            var pdfBytes = await _exportService.ExportDashboardReportToPdfAsync(dashboard);

            return File(pdfBytes, "text/html", $"dashboard-report-{DateTime.Now:yyyy-MM-dd}.html");
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Fail($"Error exporting dashboard report to PDF: {ex.Message}"));
        }
    }

    [HttpGet("dashboard/export/excel")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [Authorize(Policy = Permissions.Reports.Export)]
    public async Task<IActionResult> ExportDashboardReportToExcel([FromQuery] DashboardReportQuery request)
    {
        try
        {
            var dashboardResult = await _mediator.Send(new GetLoadReportSummaryQuery 
            { 
                StartDate = request.StartDate, 
                EndDate = request.EndDate 
            });
            
            var dashboard = new DashboardReportDto
            {
                ReportDate = DateTime.UtcNow.ToString(),
                PeriodStart = request.StartDate?.ToString() ?? DateTime.UtcNow.AddMonths(-1).ToString(),
                PeriodEnd = request.EndDate?.ToString() ?? DateTime.UtcNow.ToString(),
                LoadSummary = dashboardResult.Success ? dashboardResult.Data! : new LoadReportSummaryDto(),
                DriverSummary = new DriverReportSummaryDto(),
                FinancialSummary = new FinancialSummaryDto()
            };

            var excelBytes = await _exportService.ExportDashboardReportToExcelAsync(dashboard);

            return File(excelBytes, "text/csv", $"dashboard-report-{DateTime.Now:yyyy-MM-dd}.csv");
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Fail($"Error exporting dashboard report to Excel: {ex.Message}"));
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