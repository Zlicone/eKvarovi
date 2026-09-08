using eKvarovi.Api.Data;
using eKvarovi.Api.Models;
using eKvarovi.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly EKvaroviDbContext _db;

    public LocationsController(EKvaroviDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? locationTypeId,
        [FromQuery] bool? isActive,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir)
    {
        var query = _db.Locations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.Like(x.Name, pattern) ||
                EF.Functions.Like(x.Address, pattern) ||
                EF.Functions.Like(x.City, pattern));
        }

        if (locationTypeId.HasValue)
            query = query.Where(x => x.LocationTypeId == locationTypeId.Value);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = sortBy?.ToLowerInvariant() switch
        {
            "city" => desc ? query.OrderByDescending(x => x.City) : query.OrderBy(x => x.City),
            "type" => desc ? query.OrderByDescending(x => x.LocationType!.Name) : query.OrderBy(x => x.LocationType!.Name),
            "created" => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name)
        };

        var items = await query
            .Select(x => new LocationDto
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                City = x.City,
                LocationTypeId = x.LocationTypeId,
                LocationTypeName = x.LocationType!.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                EmployeeCount = x.Employees.Count,
                FaultReportCount = x.FaultReports.Count
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LocationDto>> GetById(int id)
    {
        var item = await _db.Locations
            .Where(x => x.Id == id)
            .Select(x => new LocationDto
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                City = x.City,
                LocationTypeId = x.LocationTypeId,
                LocationTypeName = x.LocationType!.Name,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                EmployeeCount = x.Employees.Count,
                FaultReportCount = x.FaultReports.Count
            })
            .FirstOrDefaultAsync();

        if (item is null)
            return NotFound($"Lokacija s ID-em {id} ne postoji.");

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<LocationDto>> Create(LocationCreateDto dto)
    {
        var typeExists = await _db.LocationTypes.AnyAsync(t => t.Id == dto.LocationTypeId);
        if (!typeExists)
            return BadRequest("Odabrana vrsta lokacije ne postoji.");

        var entity = new Location
        {
            Name = dto.Name.Trim(),
            Address = dto.Address.Trim(),
            City = dto.City.Trim(),
            LocationTypeId = dto.LocationTypeId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Locations.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, null);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, LocationUpdateDto dto)
    {
        var entity = await _db.Locations.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
            return NotFound($"Lokacija s ID-em {id} ne postoji.");

        var typeExists = await _db.LocationTypes.AnyAsync(t => t.Id == dto.LocationTypeId);
        if (!typeExists)
            return BadRequest("Odabrana vrsta lokacije ne postoji.");

        if (entity.IsActive && !dto.IsActive)
        {
            var hasOpenReports = await _db.FaultReports
                .AnyAsync(r => r.LocationId == id &&
                               r.Status!.Code != FaultStatusCodes.Closed);

            if (hasOpenReports)
                return BadRequest("Lokaciju nije moguće deaktivirati dok na njoj postoje nezatvorene prijave.");
        }

        entity.Name = dto.Name.Trim();
        entity.Address = dto.Address.Trim();
        entity.City = dto.City.Trim();
        entity.LocationTypeId = dto.LocationTypeId;
        entity.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Locations.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
            return NotFound($"Lokacija s ID-em {id} ne postoji.");

        var hasEmployees = await _db.Employees.AnyAsync(e => e.LocationId == id);
        var hasReports = await _db.FaultReports.AnyAsync(r => r.LocationId == id);

        if (hasEmployees || hasReports)
            return BadRequest("Lokaciju nije moguće obrisati jer na nju su vezani zaposlenici ili prijave. Umjesto brisanja, deaktivirajte je.");

        _db.Locations.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}