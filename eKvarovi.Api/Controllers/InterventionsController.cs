using eKvarovi.Api.Data;
using eKvarovi.Api.Services;
using eKvarovi.Api.Models;
using eKvarovi.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InterventionsController : ControllerBase
{
    private readonly EKvaroviDbContext _db;

    public InterventionsController(EKvaroviDbContext db)
    {
        _db = db;
    }

    private static readonly Expression<Func<Intervention, InterventionDto>> ToDto =
        i => new InterventionDto
        {
            Id = i.Id,
            FaultAssignmentId = i.FaultAssignmentId,
            FaultReportId = i.FaultAssignment!.FaultReportId,
            FaultReportTitle = i.FaultAssignment!.FaultReport!.Title,
            LocationName = i.FaultAssignment!.FaultReport!.Location!.Name,
            TechnicianName = i.FaultAssignment!.Technician!.FirstName + " " +
                             i.FaultAssignment!.Technician!.LastName,
            StatusId = i.StatusId,
            StatusCode = i.Status!.Code,
            StatusName = i.Status!.Name,
            StartedAt = i.StartedAt,
            FinishedAt = i.FinishedAt,
            DurationMinutes = i.DurationMinutes,
            Note = i.Note,
            CreatedAt = i.CreatedAt,
            MaterialCount = i.Materials.Count,
            IsEditable = i.Status!.Code == InterventionStatusCodes.Planned ||
                         i.Status!.Code == InterventionStatusCodes.InProgress
        };

    [HttpGet]
    public async Task<ActionResult<List<InterventionDto>>> GetAll(
        [FromQuery] int? faultReportId,
        [FromQuery] int? faultAssignmentId,
        [FromQuery] int? technicianId,
        [FromQuery] int? statusId,
        [FromQuery] string? search)
    {
        var query = _db.Interventions.AsQueryable();

        if (faultReportId.HasValue)
            query = query.Where(i => i.FaultAssignment!.FaultReportId == faultReportId.Value);

        if (faultAssignmentId.HasValue)
            query = query.Where(i => i.FaultAssignmentId == faultAssignmentId.Value);

        if (technicianId.HasValue)
            query = query.Where(i => i.FaultAssignment!.TechnicianId == technicianId.Value);

        if (statusId.HasValue)
            query = query.Where(i => i.StatusId == statusId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(i =>
                EF.Functions.Like(i.FaultAssignment!.FaultReport!.Title, pattern) ||
                (i.Note != null && EF.Functions.Like(i.Note, pattern)));
        }

        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Select(ToDto)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InterventionDto>> GetById(int id)
    {
        var item = await _db.Interventions
            .Where(i => i.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();

        if (item is null)
            return NotFound($"Intervencija s ID-em {id} ne postoji.");

        return Ok(item);
    }

    [HttpGet("{id:int}/materials")]
    public async Task<ActionResult<List<InterventionMaterialDto>>> GetMaterials(int id)
    {
        if (!await _db.Interventions.AnyAsync(i => i.Id == id))
            return NotFound($"Intervencija s ID-em {id} ne postoji.");

        var items = await _db.InterventionMaterials
            .Where(m => m.InterventionId == id)
            .OrderBy(m => m.Material!.Name)
            .Select(m => new InterventionMaterialDto
            {
                Id = m.Id,
                MaterialId = m.MaterialId,
                MaterialName = m.Material!.Name,
                UnitAbbreviation = m.Material!.Unit!.Abbreviation,
                Quantity = m.Quantity
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    [Authorize(Policy = "Fieldwork")]
    public async Task<ActionResult> Create(InterventionCreateDto dto)
    {
        var assignment = await _db.FaultAssignments
            .Include(a => a.FaultReport)
                .ThenInclude(r => r!.Status)
            .FirstOrDefaultAsync(a => a.Id == dto.FaultAssignmentId);

        if (assignment is null)
            return BadRequest("Odabrani radni nalog ne postoji.");

        if (assignment.UnassignedAt is not null)
            return BadRequest("Intervenciju nije moguće otvoriti na zatvorenom radnom nalogu.");

        var employeeId = User.GetEmployeeId();
        var isManager = User.IsInAnyRole(RoleNames.Admin, RoleNames.Manager);

        if (!isManager && assignment.TechnicianId != employeeId)
            return Forbid();

        if (assignment.FaultReport!.Status!.Code == FaultStatusCodes.Closed)
            return BadRequest("Prijava je zatvorena.");

        var hasOpen = await _db.Interventions.AnyAsync(i =>
            i.FaultAssignmentId == dto.FaultAssignmentId &&
            (i.Status!.Code == InterventionStatusCodes.Planned ||
             i.Status!.Code == InterventionStatusCodes.InProgress));

        if (hasOpen)
            return BadRequest("Na ovom nalogu već postoji otvorena intervencija. Prvo je završite ili označite neuspješnom.");

        var planned = await _db.InterventionStatuses
            .FirstAsync(s => s.Code == InterventionStatusCodes.Planned);

        var entity = new Intervention
        {
            FaultAssignmentId = dto.FaultAssignmentId,
            StatusId = planned.Id,
            Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Interventions.Add(entity);

        EventLogger.Log(_db, assignment.FaultReportId, "Otvorena intervencija", null, planned.Name);

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, null);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Fieldwork")]
    public async Task<IActionResult> Update(int id, InterventionUpdateDto dto)
    {
        var entity = await _db.Interventions
            .Include(i => i.Status)
            .Include(i => i.FaultAssignment)
                .ThenInclude(a => a!.FaultReport)
                    .ThenInclude(r => r!.Status)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (entity is null)
            return NotFound($"Intervencija s ID-em {id} ne postoji.");

        var employeeId = User.GetEmployeeId();
        var isManager = User.IsInAnyRole(RoleNames.Admin, RoleNames.Manager);

        if (!isManager && entity.FaultAssignment!.TechnicianId != employeeId)
            return Forbid();

        if (entity.Status!.Code is InterventionStatusCodes.Completed or InterventionStatusCodes.Failed)
            return BadRequest("Zatvorenu intervenciju nije moguće mijenjati.");

        var newStatus = await _db.InterventionStatuses.FirstOrDefaultAsync(s => s.Id == dto.StatusId);
        if (newStatus is null)
            return BadRequest("Odabrani status intervencije ne postoji.");

        if (dto.StartedAt is not null && dto.FinishedAt is not null &&
            dto.FinishedAt < dto.StartedAt)
            return BadRequest("Kraj intervencije ne može biti prije početka.");

        if (newStatus.Code == InterventionStatusCodes.InProgress && dto.StartedAt is null)
            return BadRequest("Započeta intervencija mora imati vrijeme početka.");

        if (newStatus.Code == InterventionStatusCodes.Completed)
        {
            if (dto.StartedAt is null || dto.FinishedAt is null)
                return BadRequest("Završena intervencija mora imati vrijeme početka i završetka.");

            if (string.IsNullOrWhiteSpace(dto.Note))
                return BadRequest("Završena intervencija mora imati bilješku o izvedenim radovima.");
        }

        if (newStatus.Code == InterventionStatusCodes.Failed &&
            string.IsNullOrWhiteSpace(dto.Note))
            return BadRequest("Neuspješna intervencija mora imati bilješku s razlogom.");

        var report = entity.FaultAssignment!.FaultReport!;
        var oldStatusName = entity.Status!.Name;

        entity.StatusId = newStatus.Id;
        entity.StartedAt = dto.StartedAt;
        entity.FinishedAt = dto.FinishedAt;
        entity.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();

        entity.DurationMinutes = dto.StartedAt is not null && dto.FinishedAt is not null
            ? (int)(dto.FinishedAt.Value - dto.StartedAt.Value).TotalMinutes
            : null;

        EventLogger.Log(_db, report.Id, "Status intervencije", oldStatusName, newStatus.Name);

        if (newStatus.Code == InterventionStatusCodes.InProgress &&
            report.Status!.Code == FaultStatusCodes.Assigned)
        {
            var inProgress = await _db.FaultStatuses
                .FirstAsync(s => s.Code == FaultStatusCodes.InProgress);

            EventLogger.Log(_db, report.Id, "Promjena statusa", report.Status.Name, inProgress.Name);
            report.StatusId = inProgress.Id;
        }

        if (newStatus.Code == InterventionStatusCodes.Completed &&
            report.Status!.Code is FaultStatusCodes.Assigned or FaultStatusCodes.InProgress)
        {
            var resolved = await _db.FaultStatuses
                .FirstAsync(s => s.Code == FaultStatusCodes.Resolved);

            EventLogger.Log(_db, report.Id, "Promjena statusa", report.Status.Name, resolved.Name);
            report.StatusId = resolved.Id;
            report.ResolvedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/materials")]
    [Authorize(Policy = "Fieldwork")]
    public async Task<IActionResult> AddMaterial(int id, MaterialUsageDto dto)
    {
        var intervention = await _db.Interventions
            .Include(i => i.Status)
            .Include(i => i.FaultAssignment)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (intervention is null)
            return NotFound($"Intervencija s ID-em {id} ne postoji.");

        var employeeId = User.GetEmployeeId();
        var isManager = User.IsInAnyRole(RoleNames.Admin, RoleNames.Manager);

        if (!isManager && intervention.FaultAssignment!.TechnicianId != employeeId)
            return Forbid();

        if (intervention.Status!.Code is InterventionStatusCodes.Completed or InterventionStatusCodes.Failed)
            return BadRequest("Materijal nije moguće dodati na zatvorenu intervenciju.");

        var material = await _db.Materials.FirstOrDefaultAsync(m => m.Id == dto.MaterialId);
        if (material is null)
            return BadRequest("Odabrani materijal ne postoji.");

        if (!material.IsActive)
            return BadRequest("Odabrani materijal više nije u upotrebi.");

        if (dto.Quantity <= 0)
            return BadRequest("Količina mora biti veća od nule.");

        var existing = await _db.InterventionMaterials
            .FirstOrDefaultAsync(m => m.InterventionId == id && m.MaterialId == dto.MaterialId);

        if (existing is not null)
            existing.Quantity += dto.Quantity;
        else
            _db.InterventionMaterials.Add(new InterventionMaterial
            {
                InterventionId = id,
                MaterialId = dto.MaterialId,
                Quantity = dto.Quantity
            });

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}/materials/{materialId:int}")]
    [Authorize(Policy = "Fieldwork")]
    public async Task<IActionResult> RemoveMaterial(int id, int materialId)
    {
        var intervention = await _db.Interventions
            .Include(i => i.Status)
            .Include(i => i.FaultAssignment)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (intervention is null)
            return NotFound($"Intervencija s ID-em {id} ne postoji.");

        var employeeId = User.GetEmployeeId();
        var isManager = User.IsInAnyRole(RoleNames.Admin, RoleNames.Manager);

        if (!isManager && intervention.FaultAssignment!.TechnicianId != employeeId)
            return Forbid();

        if (intervention.Status!.Code is InterventionStatusCodes.Completed or InterventionStatusCodes.Failed)
            return BadRequest("Materijal nije moguće ukloniti sa zatvorene intervencije.");

        var usage = await _db.InterventionMaterials
            .FirstOrDefaultAsync(m => m.InterventionId == id && m.MaterialId == materialId);

        if (usage is null)
            return NotFound("Traženi materijal nije evidentiran na ovoj intervenciji.");

        _db.InterventionMaterials.Remove(usage);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}