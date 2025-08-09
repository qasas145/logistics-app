using Logistics.Application.Specifications.Reports;
using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared.Models.Reports;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries.Reports.GetDriverReport;

public sealed class GetDriverReportHandler : IRequestHandler<GetDriverReportQuery, Result<PagedResult<DriverReportDto>>>
{
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<Load> _loadRepository;
    private readonly IRepository<Truck> _truckRepository;

    public GetDriverReportHandler(
        IRepository<Employee> employeeRepository,
        IRepository<Load> loadRepository,
        IRepository<Truck> truckRepository)
    {
        _employeeRepository = employeeRepository;
        _loadRepository = loadRepository;
        _truckRepository = truckRepository;
    }

    public async Task<Result<PagedResult<DriverReportDto>>> Handle(GetDriverReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var specification = new DriverReportSpecification(request);
            
            var driversQuery = _employeeRepository.GetQueryableBySpec(specification)
                .Include(e => e.Roles);

            // Get drivers (employees with driver role)
            var drivers = await driversQuery.ToListAsync(cancellationToken);
            
            // Filter drivers that have driver role
            var actualDrivers = drivers.Where(e => e.Roles.Any(r => r.Name.Contains("Driver", StringComparison.OrdinalIgnoreCase))).ToList();

            var totalCount = actualDrivers.Count;
            
            var pagedDrivers = actualDrivers
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .ToList();

            var driverReports = new List<DriverReportDto>();

            foreach (var driver in pagedDrivers)
            {
                var driverReport = await BuildDriverReportDto(driver, request, cancellationToken);
                driverReports.Add(driverReport);
            }

            // Apply sorting
            driverReports = ApplySorting(driverReports, request.SortBy, request.SortOrder);

            var result = new PagedResult<DriverReportDto>
            {
                Items = driverReports,
                TotalCount = totalCount,
                Page = request.Page,
                Size = request.Size,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.Size)
            };

            return Result<PagedResult<DriverReportDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<DriverReportDto>>.Failure($"Error generating driver report: {ex.Message}");
        }
    }

    private async Task<DriverReportDto> BuildDriverReportDto(Employee driver, GetDriverReportQuery request, CancellationToken cancellationToken)
    {
        // Get driver's truck information
        var truck = await _truckRepository.GetAll()
            .FirstOrDefaultAsync(t => t.MainDriverId == driver.Id || t.SecondaryDriverId == driver.Id, cancellationToken);

        // Get loads for this driver within the date range
        var loadsQuery = _loadRepository.GetAll()
            .Where(l => l.AssignedTruck != null && 
                       (l.AssignedTruck.MainDriverId == driver.Id || l.AssignedTruck.SecondaryDriverId == driver.Id));

        if (request.StartDate.HasValue)
            loadsQuery = loadsQuery.Where(l => l.DispatchedDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            loadsQuery = loadsQuery.Where(l => l.DispatchedDate <= request.EndDate.Value);

        var loads = await loadsQuery
            .Include(l => l.AssignedTruck)
            .Include(l => l.Customer)
            .Include(l => l.Invoices)
            .ToListAsync(cancellationToken);

        var completedLoads = loads.Where(l => l.Status == LoadStatus.Delivered).ToList();
        var inProgressLoads = loads.Where(l => l.Status == LoadStatus.PickedUp).ToList();
        var dispatchedLoads = loads.Where(l => l.Status == LoadStatus.Dispatched).ToList();

        var dto = new DriverReportDto
        {
            Id = driver.Id,
            FirstName = driver.FirstName,
            LastName = driver.LastName,
            FullName = driver.GetFullName(),
            Email = driver.Email,
            PhoneNumber = driver.PhoneNumber,
            JoinedDate = driver.JoinedDate,
            CurrentTruckNumber = truck?.Number,
            TruckModel = truck?.Type.ToString(),
            TruckStatus = truck?.Status.ToString(),
            TotalLoadsCompleted = completedLoads.Count,
            TotalLoadsInProgress = inProgressLoads.Count,
            TotalLoadsDispatched = dispatchedLoads.Count,
            TotalDistanceDriven = loads.Sum(l => l.Distance),
            TotalEarnings = loads.Sum(l => l.CalcDriverShare())
        };

        // Calculate averages
        if (loads.Count > 0)
        {
            dto.AverageDistancePerLoad = dto.TotalDistanceDriven / loads.Count;
            dto.AverageEarningsPerLoad = dto.TotalEarnings / loads.Count;
            
            if (dto.TotalDistanceDriven > 0)
                dto.AverageEarningsPerKm = dto.TotalEarnings / (decimal)dto.TotalDistanceDriven;
        }

        // Calculate on-time delivery metrics
        if (completedLoads.Count > 0)
        {
            var onTimeDeliveries = completedLoads.Count(l => IsOnTimeDelivery(l));
            dto.OnTimeDeliveryCount = onTimeDeliveries;
            dto.LateDeliveryCount = completedLoads.Count - onTimeDeliveries;
            dto.OnTimeDeliveryPercentage = (decimal)onTimeDeliveries / completedLoads.Count * 100;
            
            var deliveryTimes = completedLoads
                .Where(l => l.PickUpDate.HasValue && l.DeliveryDate.HasValue)
                .Select(l => l.DeliveryDate!.Value.Subtract(l.PickUpDate!.Value).TotalHours)
                .ToList();
            
            if (deliveryTimes.Count > 0)
                dto.AverageDeliveryTimeInHours = deliveryTimes.Average();
        }

        // Calculate period-based statistics
        dto.ThisWeek = CalculatePeriodStats(loads, GetStartOfWeek(DateTime.UtcNow), DateTime.UtcNow);
        dto.LastWeek = CalculatePeriodStats(loads, GetStartOfWeek(DateTime.UtcNow).AddDays(-7), GetStartOfWeek(DateTime.UtcNow).AddDays(-1));
        dto.ThisMonth = CalculatePeriodStats(loads, new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1), DateTime.UtcNow);
        dto.LastMonth = CalculatePeriodStats(loads, new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1), 
                                           new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddDays(-1));
        dto.ThisYear = CalculatePeriodStats(loads, new DateTime(DateTime.UtcNow.Year, 1, 1), DateTime.UtcNow);

        // Add recent loads if requested
        if (request.IncludeRecentLoads == true)
        {
            var recentLoads = loads
                .OrderByDescending(l => l.DispatchedDate)
                .Take(request.RecentLoadsLimit ?? 10)
                .ToList();
            
            dto.RecentLoads = recentLoads.Select(l => LoadReportMapper.MapToLoadReportDto(l)).ToList();
        }

        // Set last active date
        dto.LastActiveDate = loads.Any() ? loads.Max(l => l.DispatchedDate) : null;

        return dto;
    }

    private static DriverPeriodStatsDto CalculatePeriodStats(List<Load> allLoads, DateTime startDate, DateTime endDate)
    {
        var periodLoads = allLoads.Where(l => l.DispatchedDate >= startDate && l.DispatchedDate <= endDate).ToList();
        var completedLoads = periodLoads.Where(l => l.Status == LoadStatus.Delivered).ToList();

        var stats = new DriverPeriodStatsDto
        {
            LoadsCompleted = completedLoads.Count,
            TotalEarnings = periodLoads.Sum(l => l.CalcDriverShare()),
            TotalRevenue = periodLoads.Sum(l => l.DeliveryCost.Amount),
            TotalDistance = periodLoads.Sum(l => l.Distance)
        };

        if (stats.LoadsCompleted > 0)
        {
            stats.AverageEarningsPerLoad = stats.TotalEarnings / stats.LoadsCompleted;
            var onTimeDeliveries = completedLoads.Count(l => IsOnTimeDelivery(l));
            stats.OnTimeDeliveries = onTimeDeliveries;
            stats.LateDeliveries = completedLoads.Count - onTimeDeliveries;
            stats.OnTimePercentage = (decimal)onTimeDeliveries / completedLoads.Count * 100;
        }

        return stats;
    }

    private static bool IsOnTimeDelivery(Load load)
    {
        if (!load.DeliveryDate.HasValue) return false;
        var expectedDeliveryTime = load.DispatchedDate.AddHours(load.Distance / 60);
        return load.DeliveryDate.Value <= expectedDeliveryTime.AddHours(24);
    }

    private static DateTime GetStartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    private static List<DriverReportDto> ApplySorting(List<DriverReportDto> drivers, string? sortBy, string? sortOrder)
    {
        var isDescending = sortOrder?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "totalearnings" => isDescending ? drivers.OrderByDescending(d => d.TotalEarnings).ToList() : drivers.OrderBy(d => d.TotalEarnings).ToList(),
            "totalloadscompleted" => isDescending ? drivers.OrderByDescending(d => d.TotalLoadsCompleted).ToList() : drivers.OrderBy(d => d.TotalLoadsCompleted).ToList(),
            "totaldistancedriven" => isDescending ? drivers.OrderByDescending(d => d.TotalDistanceDriven).ToList() : drivers.OrderBy(d => d.TotalDistanceDriven).ToList(),
            "ontimedeliverypercentage" => isDescending ? drivers.OrderByDescending(d => d.OnTimeDeliveryPercentage).ToList() : drivers.OrderBy(d => d.OnTimeDeliveryPercentage).ToList(),
            "averageearningsperload" => isDescending ? drivers.OrderByDescending(d => d.AverageEarningsPerLoad).ToList() : drivers.OrderBy(d => d.AverageEarningsPerLoad).ToList(),
            "name" => isDescending ? drivers.OrderByDescending(d => d.FullName).ToList() : drivers.OrderBy(d => d.FullName).ToList(),
            "joineddate" => isDescending ? drivers.OrderByDescending(d => d.JoinedDate).ToList() : drivers.OrderBy(d => d.JoinedDate).ToList(),
            _ => isDescending ? drivers.OrderByDescending(d => d.TotalEarnings).ToList() : drivers.OrderBy(d => d.TotalEarnings).ToList()
        };
    }
}