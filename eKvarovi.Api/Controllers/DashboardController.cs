using eKvarovi.Api.Data;
using eKvarovi.Api.Services;
using eKvarovi.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly EKvaroviDbContext _db;

    public DashboardController(EKvaroviDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = new DashboardDto();

        result.OpenReports = await _db.FaultReports
            .CountAsync(r => r.Status!.Code != FaultStatusCodes.Closed);

        var maxRank = await _db.FaultPriorities.MaxAsync(p => p.Rank);

        result.CriticalReports = await _db.FaultReports
            .CountAsync(r => r.Priority != null &&
                             r.Priority.Rank == maxRank &&
                             r.Status!.Code != FaultStatusCodes.Closed);

        result.OverdueReports = await _db.FaultReports
            .CountAsync(r => r.DueDate != null &&
                             r.DueDate < now &&
                             r.Status!.Code != FaultStatusCodes.Resolved &&
                             r.Status!.Code != FaultStatusCodes.Closed);

        result.UnassignedReports = await _db.FaultReports
            .CountAsync(r => r.Status!.Code != FaultStatusCodes.Closed &&
                             !r.Assignments.Any(a => a.UnassignedAt == null));

        result.ActiveInterventions = await _db.Interventions
            .CountAsync(i => i.Status!.Code == InterventionStatusCodes.Planned ||
                             i.Status!.Code == InterventionStatusCodes.InProgress);

        result.ClosedThisMonth = await _db.FaultReports
            .CountAsync(r => r.ClosedAt != null && r.ClosedAt >= monthStart);

        var resolved = await _db.FaultReports
            .Where(r => r.ResolvedAt != null)
            .Select(r => new { r.CreatedAt, ResolvedAt = r.ResolvedAt!.Value })
            .ToListAsync();

        result.AverageResolutionHours = resolved.Count > 0
            ? Math.Round(resolved.Average(r => (r.ResolvedAt - r.CreatedAt).TotalHours), 1)
            : null;

        result.ByStatus = await _db.FaultStatuses
            .OrderBy(s => s.Rank)
            .Select(s => new DashboardCountDto
            {
                Label = s.Name,
                Count = s.FaultReports.Count
            })
            .ToListAsync();

        result.ByFaultType = await _db.FaultTypes
            .OrderBy(t => t.Name)
            .Select(t => new DashboardCountDto
            {
                Label = t.Name,
                Count = t.FaultReports.Count
            })
            .ToListAsync();

        result.ByLocation = await _db.Locations
            .Where(l => l.FaultReports.Any())
            .OrderByDescending(l => l.FaultReports.Count)
            .Take(5)
            .Select(l => new DashboardCountDto
            {
                Label = l.Name,
                Count = l.FaultReports.Count
            })
            .ToListAsync();

        result.LatestReports = await _db.FaultReports
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new FaultReportListDto
            {
                Id = r.Id,
                Title = r.Title,
                LocationId = r.LocationId,
                LocationName = r.Location!.Name,
                ReporterName = r.Reporter!.FirstName + " " + r.Reporter!.LastName,
                FaultTypeId = r.FaultTypeId,
                FaultTypeName = r.FaultType != null ? r.FaultType.Name : null,
                PriorityId = r.PriorityId,
                PriorityName = r.Priority != null ? r.Priority.Name : null,
                PriorityRank = r.Priority != null ? r.Priority.Rank : (int?)null,
                StatusId = r.StatusId,
                StatusCode = r.Status!.Code,
                StatusName = r.Status!.Name,
                DueDate = r.DueDate,
                CreatedAt = r.CreatedAt,
                ActiveTechnicianName = r.Assignments
                    .Where(a => a.UnassignedAt == null)
                    .Select(a => a.Technician!.FirstName + " " + a.Technician!.LastName)
                    .FirstOrDefault(),
                IsOverdue = r.DueDate != null && r.DueDate < now &&
                            r.Status!.Code != FaultStatusCodes.Resolved &&
                            r.Status!.Code != FaultStatusCodes.Closed,
                InterventionCount = r.Assignments.SelectMany(a => a.Interventions).Count(),
                AttachmentCount = r.Attachments.Count
            })
            .ToListAsync();

        var employeeId = User.GetEmployeeId();

        if (employeeId is not null)
        {
            result.Personal = new PersonalStatsDto
            {
                MyOpenReports = await _db.FaultReports
                    .CountAsync(r => r.ReporterId == employeeId.Value &&
                                     r.Status!.Code != FaultStatusCodes.Closed),

                MyResolvedReports = await _db.FaultReports
                    .CountAsync(r => r.ReporterId == employeeId.Value &&
                                     (r.Status!.Code == FaultStatusCodes.Resolved ||
                                      r.Status!.Code == FaultStatusCodes.Closed)),

                MyActiveAssignments = await _db.FaultAssignments
                    .CountAsync(a => a.TechnicianId == employeeId.Value &&
                                     a.UnassignedAt == null),

                MyOpenInterventions = await _db.Interventions
                    .CountAsync(i => i.FaultAssignment!.TechnicianId == employeeId.Value &&
                                     (i.Status!.Code == InterventionStatusCodes.Planned ||
                                      i.Status!.Code == InterventionStatusCodes.InProgress))
            };
        }

        return Ok(result);
    }

    [HttpGet("sla")]
    [Authorize(Policy = "ManageReports")]
    public async Task<ActionResult<SlaOverviewDto>> GetSla([FromQuery] int months = 6)
    {
        if (months < 1) months = 1;
        if (months > 24) months = 24;

        var now = DateTime.UtcNow;

        var reports = await _db.FaultReports
            .Select(r => new
            {
                r.Id,
                r.CreatedAt,
                r.DueDate,
                r.ResolvedAt,
                StatusCode = r.Status!.Code,
                LocationName = r.Location!.Name,
                FaultTypeName = r.FaultType != null ? r.FaultType.Name : null
            })
            .ToListAsync();

        var result = new SlaOverviewDto();

        var withDue = reports.Where(r => r.DueDate.HasValue).ToList();

        result.TotalWithDueDate = withDue.Count;

        result.MetOnTime = withDue.Count(r =>
            r.ResolvedAt.HasValue && r.ResolvedAt.Value <= r.DueDate!.Value);

        result.MissedDeadline = withDue.Count(r =>
            r.ResolvedAt.HasValue && r.ResolvedAt.Value > r.DueDate!.Value);

        result.StillOpenOverdue = withDue.Count(r =>
            !r.ResolvedAt.HasValue &&
            r.DueDate!.Value < now &&
            r.StatusCode != FaultStatusCodes.Closed);

        var closedWithDue = result.MetOnTime + result.MissedDeadline;

        result.OnTimePercentage = closedWithDue > 0
            ? Math.Round(result.MetOnTime * 100.0 / closedWithDue, 1)
            : null;

        result.ByLocation = reports
            .GroupBy(r => r.LocationName)
            .Select(g => BuildBreakdown(g.Key, g))
            .OrderByDescending(x => x.Total)
            .Take(8)
            .ToList();

        result.ByFaultType = reports
            .Where(r => r.FaultTypeName is not null)
            .GroupBy(r => r.FaultTypeName!)
            .Select(g => BuildBreakdown(g.Key, g))
            .OrderByDescending(x => x.Total)
            .ToList();

        var croatian = new[]
        {
        "sij", "velj", "ožu", "tra", "svi", "lip",
        "srp", "kol", "ruj", "lis", "stu", "pro"
    };

        var trend = new List<MonthlyTrendDto>();
        var cursor = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(-(months - 1));

        for (var i = 0; i < months; i++)
        {
            var start = cursor.AddMonths(i);
            var end = start.AddMonths(1);

            trend.Add(new MonthlyTrendDto
            {
                Year = start.Year,
                Month = start.Month,
                Label = $"{croatian[start.Month - 1]} {start.Year % 100:00}",
                Created = reports.Count(r => r.CreatedAt >= start && r.CreatedAt < end),
                Resolved = reports.Count(r =>
                    r.ResolvedAt.HasValue &&
                    r.ResolvedAt.Value >= start &&
                    r.ResolvedAt.Value < end)
            });
        }

        result.MonthlyTrend = trend;

        return Ok(result);
    }

    private static SlaBreakdownDto BuildBreakdown(
        string label,
        IEnumerable<dynamic> group)
    {
        var items = group.ToList();

        var withDue = items
            .Where(r => r.DueDate != null && r.ResolvedAt != null)
            .ToList();

        var onTime = withDue.Count(r => r.ResolvedAt <= r.DueDate);

        var resolved = items
            .Where(r => r.ResolvedAt != null)
            .Select(r => (double)((DateTime)r.ResolvedAt - (DateTime)r.CreatedAt).TotalHours)
            .ToList();

        return new SlaBreakdownDto
        {
            Label = label,
            Total = items.Count,
            WithDueDate = withDue.Count,
            OnTime = onTime,
            OnTimePercentage = withDue.Count > 0
                ? Math.Round(onTime * 100.0 / withDue.Count, 1)
                : null,
            AverageResolutionHours = resolved.Count > 0
                ? Math.Round(resolved.Average(), 1)
                : null
        };
    }
}