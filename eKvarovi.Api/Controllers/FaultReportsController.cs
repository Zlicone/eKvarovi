using eKvarovi.Api.Data;
using eKvarovi.Api.Models;
using eKvarovi.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaultReportsController : ControllerBase
{
    private readonly EKvaroviDbContext _db;

    public FaultReportsController(EKvaroviDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<FaultReportListDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? statusId,
        [FromQuery] int? priorityId,
        [FromQuery] int? faultTypeId,
        [FromQuery] int? locationId,
        [FromQuery] int? technicianId,
        [FromQuery] bool? unassigned,
        [FromQuery] bool? overdue,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir)
    {
        var now = DateTime.UtcNow;
        var query = _db.FaultReports.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.Like(x.Title, pattern) ||
                EF.Functions.Like(x.Description, pattern));
        }

        if (statusId.HasValue)
            query = query.Where(x => x.StatusId == statusId.Value);

        if (priorityId.HasValue)
            query = query.Where(x => x.PriorityId == priorityId.Value);

        if (faultTypeId.HasValue)
            query = query.Where(x => x.FaultTypeId == faultTypeId.Value);

        if (locationId.HasValue)
            query = query.Where(x => x.LocationId == locationId.Value);

        if (technicianId.HasValue)
            query = query.Where(x => x.Assignments
                .Any(a => a.UnassignedAt == null && a.TechnicianId == technicianId.Value));

        if (unassigned == true)
            query = query.Where(x => !x.Assignments.Any(a => a.UnassignedAt == null));

        if (overdue == true)
            query = query.Where(x =>
                x.DueDate != null &&
                x.DueDate < now &&
                x.Status!.Code != FaultStatusCodes.Resolved &&
                x.Status!.Code != FaultStatusCodes.Closed);

        if (from.HasValue)
            query = query.Where(x => x.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.CreatedAt <= to.Value);

        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = sortBy?.ToLowerInvariant() switch
        {
            "title" => desc ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title),
            "priority" => desc
                ? query.OrderByDescending(x => x.Priority!.Rank)
                : query.OrderBy(x => x.Priority!.Rank),
            "status" => desc
                ? query.OrderByDescending(x => x.Status!.Rank)
                : query.OrderBy(x => x.Status!.Rank),
            "duedate" => desc ? query.OrderByDescending(x => x.DueDate) : query.OrderBy(x => x.DueDate),
            _ => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
        };

        if (string.IsNullOrWhiteSpace(sortBy))
            query = query.OrderByDescending(x => x.CreatedAt);

        var items = await query
            .Select(x => new FaultReportListDto
            {
                Id = x.Id,
                Title = x.Title,
                LocationId = x.LocationId,
                LocationName = x.Location!.Name,
                ReporterName = x.Reporter!.FirstName + " " + x.Reporter!.LastName,
                FaultTypeId = x.FaultTypeId,
                FaultTypeName = x.FaultType != null ? x.FaultType.Name : null,
                PriorityId = x.PriorityId,
                PriorityName = x.Priority != null ? x.Priority.Name : null,
                PriorityRank = x.Priority != null ? x.Priority.Rank : (int?)null,
                StatusId = x.StatusId,
                StatusCode = x.Status!.Code,
                StatusName = x.Status!.Name,
                DueDate = x.DueDate,
                CreatedAt = x.CreatedAt,
                ActiveTechnicianName = x.Assignments
                    .Where(a => a.UnassignedAt == null)
                    .Select(a => a.Technician!.FirstName + " " + a.Technician!.LastName)
                    .FirstOrDefault(),
                IsOverdue = x.DueDate != null &&
                            x.DueDate < now &&
                            x.Status!.Code != FaultStatusCodes.Resolved &&
                            x.Status!.Code != FaultStatusCodes.Closed,
                InterventionCount = x.Assignments.SelectMany(a => a.Interventions).Count(),
                AttachmentCount = x.Attachments.Count
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FaultReportListDto>> GetById(int id)
    {
        var now = DateTime.UtcNow;

        var item = await _db.FaultReports
            .Where(x => x.Id == id)
            .Select(x => new FaultReportListDto
            {
                Id = x.Id,
                Title = x.Title,
                LocationId = x.LocationId,
                LocationName = x.Location!.Name,
                ReporterName = x.Reporter!.FirstName + " " + x.Reporter!.LastName,
                FaultTypeId = x.FaultTypeId,
                FaultTypeName = x.FaultType != null ? x.FaultType.Name : null,
                PriorityId = x.PriorityId,
                PriorityName = x.Priority != null ? x.Priority.Name : null,
                PriorityRank = x.Priority != null ? x.Priority.Rank : (int?)null,
                StatusId = x.StatusId,
                StatusCode = x.Status!.Code,
                StatusName = x.Status!.Name,
                DueDate = x.DueDate,
                CreatedAt = x.CreatedAt,
                ActiveTechnicianName = x.Assignments
                    .Where(a => a.UnassignedAt == null)
                    .Select(a => a.Technician!.FirstName + " " + a.Technician!.LastName)
                    .FirstOrDefault(),
                IsOverdue = x.DueDate != null &&
                            x.DueDate < now &&
                            x.Status!.Code != FaultStatusCodes.Resolved &&
                            x.Status!.Code != FaultStatusCodes.Closed,
                InterventionCount = x.Assignments.SelectMany(a => a.Interventions).Count(),
                AttachmentCount = x.Attachments.Count
            })
            .FirstOrDefaultAsync();

        if (item is null)
            return NotFound($"Prijava s ID-em {id} ne postoji.");

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult> Create(FaultReportCreateDto dto)
    {
        var location = await _db.Locations.FirstOrDefaultAsync(l => l.Id == dto.LocationId);
        if (location is null)
            return BadRequest("Odabrana lokacija ne postoji.");

        if (!location.IsActive)
            return BadRequest("Prijavu nije moguće otvoriti na neaktivnoj lokaciji.");

        var reporter = await _db.Employees.FirstOrDefaultAsync(e => e.Id == dto.ReporterId);
        if (reporter is null)
            return BadRequest("Odabrani prijavitelj ne postoji.");

        if (!reporter.IsActive)
            return BadRequest("Neaktivan zaposlenik ne može otvoriti prijavu.");

        var received = await _db.FaultStatuses
            .FirstAsync(s => s.Code == FaultStatusCodes.Received);

        var entity = new FaultReport
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            LocationId = dto.LocationId,
            ReporterId = dto.ReporterId,
            StatusId = received.Id,
            CreatedAt = DateTime.UtcNow
        };

        _db.FaultReports.Add(entity);
        await _db.SaveChangesAsync();

        EventLogger.Log(_db, entity.Id, "Otvaranje prijave", null, received.Name);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, null);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, FaultReportUpdateDto dto)
    {
        var entity = await _db.FaultReports
            .Include(x => x.Status)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return NotFound($"Prijava s ID-em {id} ne postoji.");

        if (entity.Status!.Code is not (FaultStatusCodes.Received or FaultStatusCodes.Reviewed))
            return BadRequest("Sadržaj prijave moguće je mijenjati samo dok nije dodijeljena izvršitelju.");

        var location = await _db.Locations.FirstOrDefaultAsync(l => l.Id == dto.LocationId);
        if (location is null)
            return BadRequest("Odabrana lokacija ne postoji.");

        if (!location.IsActive && location.Id != entity.LocationId)
            return BadRequest("Prijavu nije moguće premjestiti na neaktivnu lokaciju.");

        entity.Title = dto.Title.Trim();
        entity.Description = dto.Description.Trim();
        entity.LocationId = dto.LocationId;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/review")]
    public async Task<IActionResult> Review(int id, FaultReportReviewDto dto)
    {
        var entity = await _db.FaultReports
            .Include(x => x.Status)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return NotFound($"Prijava s ID-em {id} ne postoji.");

        if (entity.Status!.Code == FaultStatusCodes.Closed)
            return BadRequest("Zatvorenu prijavu nije moguće mijenjati.");

        if (!await _db.FaultTypes.AnyAsync(t => t.Id == dto.FaultTypeId))
            return BadRequest("Odabrana vrsta kvara ne postoji.");

        var priority = await _db.FaultPriorities.FirstOrDefaultAsync(p => p.Id == dto.PriorityId);
        if (priority is null)
            return BadRequest("Odabrani prioritet ne postoji.");

        var maxRank = await _db.FaultPriorities.MaxAsync(p => p.Rank);

        if (priority.Rank == maxRank && dto.DueDate is null)
            return BadRequest("Prijava kritičnog prioriteta mora imati postavljen rok rješavanja.");

        if (dto.DueDate is not null && dto.DueDate < entity.CreatedAt)
            return BadRequest("Rok rješavanja ne može biti prije datuma prijave.");

        var oldPriority = entity.Priority?.Name;

        entity.FaultTypeId = dto.FaultTypeId;
        entity.PriorityId = dto.PriorityId;
        entity.DueDate = dto.DueDate;

        if (entity.Status!.Code == FaultStatusCodes.Received)
        {
            var reviewed = await _db.FaultStatuses
                .FirstAsync(s => s.Code == FaultStatusCodes.Reviewed);

            entity.StatusId = reviewed.Id;
            entity.ReviewedAt = DateTime.UtcNow;

            EventLogger.Log(_db, entity.Id, "Promjena statusa",
                entity.Status.Name, reviewed.Name);
        }

        EventLogger.Log(_db, entity.Id, "Promjena prioriteta", oldPriority, priority.Name);

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> Close(int id, [FromQuery] int closedByEmployeeId)
    {
        var entity = await _db.FaultReports
            .Include(x => x.Status)
            .Include(x => x.Assignments)
                .ThenInclude(a => a.Interventions)
                    .ThenInclude(i => i.Status)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return NotFound($"Prijava s ID-em {id} ne postoji.");

        if (entity.Status!.Code == FaultStatusCodes.Closed)
            return BadRequest("Prijava je već zatvorena.");

        var hasCompleted = entity.Assignments
            .SelectMany(a => a.Interventions)
            .Any(i => i.Status!.Code == InterventionStatusCodes.Completed);

        if (!hasCompleted)
            return BadRequest("Prijavu je moguće zatvoriti tek nakon barem jedne uspješno završene intervencije.");

        if (!await _db.Employees.AnyAsync(e => e.Id == closedByEmployeeId))
            return BadRequest("Zaposlenik koji zatvara prijavu ne postoji.");

        var closed = await _db.FaultStatuses
            .FirstAsync(s => s.Code == FaultStatusCodes.Closed);

        EventLogger.Log(_db, entity.Id, "Promjena statusa", entity.Status.Name, closed.Name);

        entity.StatusId = closed.Id;
        entity.ClosedAt = DateTime.UtcNow;
        entity.ClosedByEmployeeId = closedByEmployeeId;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.FaultReports
            .Include(x => x.Status)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return NotFound($"Prijava s ID-em {id} ne postoji.");

        var hasAssignments = await _db.FaultAssignments.AnyAsync(a => a.FaultReportId == id);

        if (hasAssignments)
            return BadRequest("Prijavu nije moguće obrisati jer za nju postoji povijest dodjela i intervencija.");

        if (entity.Status!.Code != FaultStatusCodes.Received)
            return BadRequest("Moguće je obrisati samo prijavu koja još nije obrađena.");

        var events = _db.FaultReportEvents.Where(e => e.FaultReportId == id);
        _db.FaultReportEvents.RemoveRange(events);

        _db.FaultReports.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:int}/detail")]
    public async Task<ActionResult<FaultReportDetailDto>> GetDetail(int id)
    {
        var now = DateTime.UtcNow;

        var item = await _db.FaultReports
            .Where(x => x.Id == id)
            .Select(x => new FaultReportDetailDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                LocationId = x.LocationId,
                LocationName = x.Location!.Name,
                LocationAddress = x.Location!.Address,
                LocationCity = x.Location!.City,
                ReporterId = x.ReporterId,
                ReporterName = x.Reporter!.FirstName + " " + x.Reporter!.LastName,
                ReporterEmail = x.Reporter!.Email,
                FaultTypeId = x.FaultTypeId,
                FaultTypeName = x.FaultType != null ? x.FaultType.Name : null,
                PriorityId = x.PriorityId,
                PriorityName = x.Priority != null ? x.Priority.Name : null,
                PriorityRank = x.Priority != null ? x.Priority.Rank : (int?)null,
                StatusId = x.StatusId,
                StatusCode = x.Status!.Code,
                StatusName = x.Status!.Name,
                DueDate = x.DueDate,
                CreatedAt = x.CreatedAt,
                ReviewedAt = x.ReviewedAt,
                ResolvedAt = x.ResolvedAt,
                ClosedAt = x.ClosedAt,
                ClosedByName = x.ClosedByEmployee != null
                    ? x.ClosedByEmployee.FirstName + " " + x.ClosedByEmployee.LastName
                    : null,
                ActiveTechnicianName = x.Assignments
                    .Where(a => a.UnassignedAt == null)
                    .Select(a => a.Technician!.FirstName + " " + a.Technician!.LastName)
                    .FirstOrDefault(),
                IsOverdue = x.DueDate != null &&
                            x.DueDate < now &&
                            x.Status!.Code != FaultStatusCodes.Resolved &&
                            x.Status!.Code != FaultStatusCodes.Closed,
                CanBeClosed = x.Status!.Code != FaultStatusCodes.Closed &&
                              x.Assignments.SelectMany(a => a.Interventions)
                                  .Any(i => i.Status!.Code == InterventionStatusCodes.Completed)
            })
            .FirstOrDefaultAsync();

        if (item is null)
            return NotFound($"Prijava s ID-em {id} ne postoji.");

        return Ok(item);
    }

    [HttpGet("{id:int}/events")]
    public async Task<ActionResult<List<FaultReportEventDto>>> GetEvents(int id)
    {
        if (!await _db.FaultReports.AnyAsync(r => r.Id == id))
            return NotFound($"Prijava s ID-em {id} ne postoji.");

        var items = await _db.FaultReportEvents
            .Where(e => e.FaultReportId == id)
            .OrderByDescending(e => e.ChangedAt)
            .ThenByDescending(e => e.Id)
            .Select(e => new FaultReportEventDto
            {
                Id = e.Id,
                EventType = e.EventType,
                OldValue = e.OldValue,
                NewValue = e.NewValue,
                ChangedAt = e.ChangedAt
            })
            .ToListAsync();

        return Ok(items);
    }
}