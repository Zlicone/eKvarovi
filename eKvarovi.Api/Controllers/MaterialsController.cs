using eKvarovi.Api.Data;
using eKvarovi.Api.Models;
using eKvarovi.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaterialsController : ControllerBase
{
    private readonly EKvaroviDbContext _db;

    public MaterialsController(EKvaroviDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<MaterialDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? unitId,
        [FromQuery] bool? isActive)
    {
        var query = _db.Materials.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(m => EF.Functions.Like(m.Name, pattern));
        }

        if (unitId.HasValue)
            query = query.Where(m => m.UnitId == unitId.Value);

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        var items = await query
            .OrderBy(m => m.Name)
            .Select(m => new MaterialDto
            {
                Id = m.Id,
                Name = m.Name,
                UnitId = m.UnitId,
                UnitName = m.Unit!.Name,
                UnitAbbreviation = m.Unit!.Abbreviation,
                IsActive = m.IsActive,
                UsageCount = m.InterventionMaterials.Count
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MaterialDto>> GetById(int id)
    {
        var item = await _db.Materials
            .Where(m => m.Id == id)
            .Select(m => new MaterialDto
            {
                Id = m.Id,
                Name = m.Name,
                UnitId = m.UnitId,
                UnitName = m.Unit!.Name,
                UnitAbbreviation = m.Unit!.Abbreviation,
                IsActive = m.IsActive,
                UsageCount = m.InterventionMaterials.Count
            })
            .FirstOrDefaultAsync();

        if (item is null)
            return NotFound($"Materijal s ID-em {id} ne postoji.");

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult> Create(MaterialCreateDto dto)
    {
        var name = dto.Name.Trim();

        if (await _db.Materials.AnyAsync(m => m.Name.ToLower() == name.ToLower()))
            return BadRequest("Materijal s tim nazivom već postoji.");

        if (!await _db.MaterialUnits.AnyAsync(u => u.Id == dto.UnitId))
            return BadRequest("Odabrana mjerna jedinica ne postoji.");

        var entity = new Material
        {
            Name = name,
            UnitId = dto.UnitId,
            IsActive = true
        };

        _db.Materials.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, null);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, MaterialUpdateDto dto)
    {
        var entity = await _db.Materials.FirstOrDefaultAsync(m => m.Id == id);
        if (entity is null)
            return NotFound($"Materijal s ID-em {id} ne postoji.");

        var name = dto.Name.Trim();

        if (await _db.Materials.AnyAsync(m => m.Id != id && m.Name.ToLower() == name.ToLower()))
            return BadRequest("Materijal s tim nazivom već postoji.");

        if (!await _db.MaterialUnits.AnyAsync(u => u.Id == dto.UnitId))
            return BadRequest("Odabrana mjerna jedinica ne postoji.");

        var isUsed = await _db.InterventionMaterials.AnyAsync(m => m.MaterialId == id);

        if (isUsed && entity.UnitId != dto.UnitId)
            return BadRequest("Mjernu jedinicu nije moguće promijeniti jer je materijal već evidentiran na intervencijama.");

        entity.Name = name;
        entity.UnitId = dto.UnitId;
        entity.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Materials.FirstOrDefaultAsync(m => m.Id == id);
        if (entity is null)
            return NotFound($"Materijal s ID-em {id} ne postoji.");

        if (await _db.InterventionMaterials.AnyAsync(m => m.MaterialId == id))
            return BadRequest("Materijal nije moguće obrisati jer je evidentiran na intervencijama. Umjesto brisanja, označite ga neaktivnim.");

        _db.Materials.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}