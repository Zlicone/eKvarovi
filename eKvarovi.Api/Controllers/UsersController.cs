using eKvarovi.Api.Data;
using eKvarovi.Api.Models;
using eKvarovi.Api.Services;
using eKvarovi.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly EKvaroviDbContext _db;
    private readonly IPasswordHasher<AppUser> _hasher;

    public UsersController(EKvaroviDbContext db, IPasswordHasher<AppUser> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    [HttpGet]
    public async Task<ActionResult<List<AppUserDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive)
    {
        var query = _db.AppUsers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(u =>
                EF.Functions.Like(u.Username, pattern) ||
                EF.Functions.Like(u.Email, pattern));
        }

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        var items = await query
            .OrderBy(u => u.Username)
            .Select(u => new AppUserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                EmployeeId = u.EmployeeId,
                EmployeeName = u.Employee != null
                    ? u.Employee.FirstName + " " + u.Employee.LastName
                    : null,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                Roles = u.UserRoles.Select(ur => ur.AppRole!.Name).ToList()
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("roles")]
    public async Task<ActionResult<List<LookupDto>>> GetRoles()
    {
        var items = await _db.AppRoles
            .OrderBy(r => r.Id)
            .Select(r => new LookupDto { Id = r.Id, Name = r.Name })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppUserDto>> GetById(int id)
    {
        var item = await _db.AppUsers
            .Where(u => u.Id == id)
            .Select(u => new AppUserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                EmployeeId = u.EmployeeId,
                EmployeeName = u.Employee != null
                    ? u.Employee.FirstName + " " + u.Employee.LastName
                    : null,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                Roles = u.UserRoles.Select(ur => ur.AppRole!.Name).ToList()
            })
            .FirstOrDefaultAsync();

        if (item is null)
            return NotFound($"Korisnik s ID-em {id} ne postoji.");

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult> Create(AppUserCreateDto dto)
    {
        var username = dto.Username.Trim().ToLowerInvariant();
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _db.AppUsers.AnyAsync(u => u.Username.ToLower() == username))
            return BadRequest("Korisničko ime je već zauzeto.");

        if (await _db.AppUsers.AnyAsync(u => u.Email.ToLower() == email))
            return BadRequest("Korisnik s tim emailom već postoji.");

        if (dto.EmployeeId.HasValue)
        {
            if (!await _db.Employees.AnyAsync(e => e.Id == dto.EmployeeId.Value))
                return BadRequest("Odabrani zaposlenik ne postoji.");

            if (await _db.AppUsers.AnyAsync(u => u.EmployeeId == dto.EmployeeId.Value))
                return BadRequest("Taj zaposlenik već ima korisnički račun.");
        }

        var validRoleIds = await _db.AppRoles
            .Where(r => dto.RoleIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync();

        if (validRoleIds.Count != dto.RoleIds.Distinct().Count())
            return BadRequest("Jedna ili više odabranih uloga ne postoji.");

        if (validRoleIds.Count == 0)
            return BadRequest("Potrebno je odabrati barem jednu ulogu.");

        var user = new AppUser
        {
            Username = username,
            Email = email,
            EmployeeId = dto.EmployeeId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _hasher.HashPassword(user, dto.Password);

        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();

        foreach (var roleId in validRoleIds)
            _db.AppUserRoles.Add(new AppUserRole { AppUserId = user.Id, AppRoleId = roleId });

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, null);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AppUserUpdateDto dto)
    {
        var user = await _db.AppUsers
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return NotFound($"Korisnik s ID-em {id} ne postoji.");

        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _db.AppUsers.AnyAsync(u => u.Id != id && u.Email.ToLower() == email))
            return BadRequest("Korisnik s tim emailom već postoji.");

        if (dto.EmployeeId.HasValue)
        {
            if (!await _db.Employees.AnyAsync(e => e.Id == dto.EmployeeId.Value))
                return BadRequest("Odabrani zaposlenik ne postoji.");

            if (await _db.AppUsers.AnyAsync(u => u.Id != id && u.EmployeeId == dto.EmployeeId.Value))
                return BadRequest("Taj zaposlenik već ima korisnički račun.");
        }

        var validRoleIds = await _db.AppRoles
            .Where(r => dto.RoleIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync();

        if (validRoleIds.Count == 0)
            return BadRequest("Potrebno je odabrati barem jednu ulogu.");

        var currentUserId = User.GetUserId();
        var adminRoleId = await _db.AppRoles
            .Where(r => r.Name == RoleNames.Admin)
            .Select(r => r.Id)
            .FirstAsync();

        var losesAdmin = user.UserRoles.Any(ur => ur.AppRoleId == adminRoleId) &&
                         !validRoleIds.Contains(adminRoleId);

        if (id == currentUserId && (losesAdmin || !dto.IsActive))
            return BadRequest("Ne možete sebi ukloniti administratorsku ulogu niti deaktivirati vlastiti račun.");

        if (losesAdmin)
        {
            var otherAdmins = await _db.AppUserRoles
                .CountAsync(ur => ur.AppRoleId == adminRoleId &&
                                  ur.AppUserId != id &&
                                  ur.AppUser!.IsActive);

            if (otherAdmins == 0)
                return BadRequest("U sustavu mora ostati barem jedan aktivan administrator.");
        }

        user.Email = email;
        user.EmployeeId = dto.EmployeeId;
        user.IsActive = dto.IsActive;

        _db.AppUserRoles.RemoveRange(user.UserRoles);
        await _db.SaveChangesAsync();

        foreach (var roleId in validRoleIds)
            _db.AppUserRoles.Add(new AppUserRole { AppUserId = user.Id, AppRoleId = roleId });

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/password")]
    public async Task<IActionResult> ChangePassword(int id, ChangePasswordDto dto)
    {
        var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return NotFound($"Korisnik s ID-em {id} ne postoji.");

        user.PasswordHash = _hasher.HashPassword(user, dto.NewPassword);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.AppUsers
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return NotFound($"Korisnik s ID-em {id} ne postoji.");

        if (id == User.GetUserId())
            return BadRequest("Ne možete obrisati vlastiti račun.");

        var hasUploads = await _db.FaultAttachments.AnyAsync(a => a.UploadedByAppUserId == id);
        var hasEvents = await _db.FaultReportEvents.AnyAsync(e => e.ChangedByAppUserId == id);

        if (hasUploads || hasEvents)
            return BadRequest("Račun nije moguće obrisati jer postoji njegov trag u sustavu. Umjesto brisanja, deaktivirajte ga.");

        _db.AppUserRoles.RemoveRange(user.UserRoles);
        _db.AppUsers.Remove(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}