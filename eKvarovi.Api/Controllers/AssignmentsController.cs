using eKvarovi.Api.Data;
using eKvarovi.Api.Models;
using eKvarovi.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentsController : ControllerBase
{
    private readonly EKvaroviDbContext _db;

    public AssignmentsController(EKvaroviDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<AssignmentDto>>> GetAll(
        [FromQuery] int? faultReportId,
        [FromQuery] int? technicianId,
        [FromQuery] bool? activeOnly,
        [FromQuery] string? search)
    {
        var query = _db.FaultAssignments.AsQueryable();

        if (faultReportId.HasValue)
            query = query.Where(a => a.FaultReportId == faultReportId.Value);

        if (technicianId.HasValue)
            query = query.Where(a => a.TechnicianId == technicianId.Value);

        if (activeOnly == true)
            query = query.Where(a => a.UnassignedAt == null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(a =>
                EF.Functions.Like(a.FaultReport!.Title, pattern) ||
                EF.Functions.Like(a.Technician!.FirstName, pattern) ||
                EF.Functions.Like(a.Technician!.LastName, pattern));
        }

        var items = await query
            .OrderByDescending(a => a.UnassignedAt == null)
            .ThenByDescending(a => a.AssignedAt)
            .Select(ToDto)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssignmentDto>> GetById(int id)
    {
        var item = await _db.FaultAssignments
            .Where(a => a.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();

        if (item is null)
            return NotFound($"Radni nalog s ID-em {id} ne postoji.");

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult> Create(AssignmentCreateDto dto)
    {
        var report = await _db.FaultReports
            .Include(r => r.Status)
            .Include(r => r.Assignments)
            .FirstOrDefaultAsync(r => r.Id == dto.FaultReportId);

        if (report is null)
            return BadRequest("Odabrana prijava ne postoji.");

        if (report.Status!.Code == FaultStatusCodes.Closed)
            return BadRequest("Zatvorenoj prijavi nije moguće dodijeliti izvršitelja.");

        if (report.Status!.Code == FaultStatusCodes.Received)
            return BadRequest("Prijavu je prije dodjele potrebno pregledati i odrediti vrstu i prioritet.");

        var technician = await _db.Employees.FirstOrDefaultAsync(e => e.Id == dto.TechnicianId);
        if (technician is null)
            return BadRequest("Odabrani izvršitelj ne postoji.");

        if (!technician.IsActive)
            return BadRequest("Nalog nije moguće dodijeliti neaktivnom zaposleniku.");

        var current = report.Assignments.FirstOrDefault(a => a.UnassignedAt == null);

        if (current is not null && current.TechnicianId == dto.TechnicianId)
            return BadRequest("Prijava je već dodijeljena tom izvršitelju.");

        var now = DateTime.UtcNow;

        if (current is not null)
        {
            current.UnassignedAt = now;

            var previous = await _db.Employees
                .Where(e => e.Id == current.TechnicianId)
                .Select(e => e.FirstName + " " + e.LastName)
                .FirstAsync();

            EventLogger.Log(_db, report.Id, "Ponovna dodjela",
                previous, technician.FirstName + " " + technician.LastName);
        }
        else
        {
            EventLogger.Log(_db, report.Id, "Dodjela naloga",
                null, technician.FirstName + " " + technician.LastName);
        }

        _db.FaultAssignments.Add(new FaultAssignment
        {
            FaultReportId = dto.FaultReportId,
            TechnicianId = dto.TechnicianId,
            AssignedByEmployeeId = dto.AssignedByEmployeeId,
            AssignedAt = now,
            Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim()
        });

        if (report.Status!.Code == FaultStatusCodes.Reviewed)
        {
            var assigned = await _db.FaultStatuses
                .FirstAsync(s => s.Code == FaultStatusCodes.Assigned);

            EventLogger.Log(_db, report.Id, "Promjena statusa",
                report.Status.Name, assigned.Name);

            report.StatusId = assigned.Id;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{id:int}/unassign")]
    public async Task<IActionResult> Unassign(int id)
    {
        var assignment = await _db.FaultAssignments
            .Include(a => a.FaultReport)
                .ThenInclude(r => r!.Status)
            .Include(a => a.Interventions)
                .ThenInclude(i => i.Status)
            .Include(a => a.Technician)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assignment is null)
            return NotFound($"Radni nalog s ID-em {id} ne postoji.");

        if (assignment.UnassignedAt is not null)
            return BadRequest("Nalog je već zatvoren.");

        var hasOpenIntervention = assignment.Interventions
            .Any(i => i.Status!.Code == InterventionStatusCodes.Planned ||
                      i.Status!.Code == InterventionStatusCodes.InProgress);

        if (hasOpenIntervention)
            return BadRequest("Nalog nije moguće skinuti dok postoji planirana ili započeta intervencija. Prvo zatvorite intervenciju.");

        assignment.UnassignedAt = DateTime.UtcNow;

        EventLogger.Log(_db, assignment.FaultReportId, "Skidanje naloga",
            assignment.Technician!.FirstName + " " + assignment.Technician!.LastName, null);

        var report = assignment.FaultReport!;

        if (report.Status!.Code is FaultStatusCodes.Assigned or FaultStatusCodes.InProgress)
        {
            var reviewed = await _db.FaultStatuses
                .FirstAsync(s => s.Code == FaultStatusCodes.Reviewed);

            EventLogger.Log(_db, report.Id, "Promjena statusa",
                report.Status.Name, reviewed.Name);

            report.StatusId = reviewed.Id;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static readonly Expression<Func<FaultAssignment, AssignmentDto>> ToDto =
    a => new AssignmentDto
    {
        Id = a.Id,
        FaultReportId = a.FaultReportId,
        FaultReportTitle = a.FaultReport!.Title,
        LocationName = a.FaultReport!.Location!.Name,
        PriorityName = a.FaultReport!.Priority != null ? a.FaultReport!.Priority.Name : null,
        PriorityRank = a.FaultReport!.Priority != null ? a.FaultReport!.Priority.Rank : (int?)null,
        ReportStatusCode = a.FaultReport!.Status!.Code,
        ReportStatusName = a.FaultReport!.Status!.Name,
        DueDate = a.FaultReport!.DueDate,
        TechnicianId = a.TechnicianId,
        TechnicianName = a.Technician!.FirstName + " " + a.Technician!.LastName,
        AssignedByName = a.AssignedByEmployee != null
            ? a.AssignedByEmployee.FirstName + " " + a.AssignedByEmployee.LastName
            : null,
        AssignedAt = a.AssignedAt,
        UnassignedAt = a.UnassignedAt,
        IsActive = a.UnassignedAt == null,
        Note = a.Note,
        InterventionCount = a.Interventions.Count,
        HasCompletedIntervention = a.Interventions
            .Any(i => i.Status!.Code == InterventionStatusCodes.Completed)
    };
}