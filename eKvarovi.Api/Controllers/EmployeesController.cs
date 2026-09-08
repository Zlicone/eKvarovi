using eKvarovi.Api.Data;
using eKvarovi.Api.Models;
using eKvarovi.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly EKvaroviDbContext _db;

    public EmployeesController(EKvaroviDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? locationId,
        [FromQuery] bool? isActive)
    {
        var query = _db.Employees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.Like(x.FirstName, pattern) ||
                EF.Functions.Like(x.LastName, pattern) ||
                EF.Functions.Like(x.Email, pattern));
        }

        if (locationId.HasValue)
            query = query.Where(x => x.LocationId == locationId.Value);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        var items = await query
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .Select(x => new EmployeeDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                Phone = x.Phone,
                LocationId = x.LocationId,
                LocationName = x.Location!.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                HasAccount = x.AppUser != null,
                ReportedCount = x.ReportedFaults.Count,
                ActiveAssignmentCount = x.Assignments.Count(a => a.UnassignedAt == null)
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var item = await _db.Employees
            .Where(x => x.Id == id)
            .Select(x => new EmployeeDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                Phone = x.Phone,
                LocationId = x.LocationId,
                LocationName = x.Location!.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                HasAccount = x.AppUser != null,
                ReportedCount = x.ReportedFaults.Count,
                ActiveAssignmentCount = x.Assignments.Count(a => a.UnassignedAt == null)
            })
            .FirstOrDefaultAsync();

        if (item is null)
            return NotFound($"Zaposlenik s ID-em {id} ne postoji.");

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult> Create(EmployeeCreateDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _db.Employees.AnyAsync(e => e.Email.ToLower() == email))
            return BadRequest("Zaposlenik s tim emailom već postoji.");

        var location = await _db.Locations.FirstOrDefaultAsync(l => l.Id == dto.LocationId);
        if (location is null)
            return BadRequest("Odabrana lokacija ne postoji.");

        if (!location.IsActive)
            return BadRequest("Zaposlenika nije moguće dodati na neaktivnu lokaciju.");

        var entity = new Employee
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
            LocationId = dto.LocationId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Employees.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, null);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, EmployeeUpdateDto dto)
    {
        var entity = await _db.Employees.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
            return NotFound($"Zaposlenik s ID-em {id} ne postoji.");

        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _db.Employees.AnyAsync(e => e.Id != id && e.Email.ToLower() == email))
            return BadRequest("Zaposlenik s tim emailom već postoji.");

        var location = await _db.Locations.FirstOrDefaultAsync(l => l.Id == dto.LocationId);
        if (location is null)
            return BadRequest("Odabrana lokacija ne postoji.");

        if (!location.IsActive && location.Id != entity.LocationId)
            return BadRequest("Zaposlenika nije moguće premjestiti na neaktivnu lokaciju.");

        if (entity.IsActive && !dto.IsActive)
        {
            var hasActiveAssignment = await _db.FaultAssignments
                .AnyAsync(a => a.TechnicianId == id && a.UnassignedAt == null);

            if (hasActiveAssignment)
                return BadRequest("Zaposlenika nije moguće deaktivirati dok ima aktivan radni nalog. Prvo prebacite nalog na drugog izvršitelja.");
        }

        entity.FirstName = dto.FirstName.Trim();
        entity.LastName = dto.LastName.Trim();
        entity.Email = email;
        entity.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        entity.LocationId = dto.LocationId;
        entity.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Employees.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
            return NotFound($"Zaposlenik s ID-em {id} ne postoji.");

        var hasReports = await _db.FaultReports.AnyAsync(r => r.ReporterId == id);
        var hasAssignments = await _db.FaultAssignments.AnyAsync(a => a.TechnicianId == id);
        var hasAccount = await _db.AppUsers.AnyAsync(u => u.EmployeeId == id);

        if (hasReports || hasAssignments || hasAccount)
            return BadRequest("Zaposlenika nije moguće obrisati jer postoji njegova povijest u sustavu. Umjesto brisanja, deaktivirajte ga.");

        _db.Employees.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}