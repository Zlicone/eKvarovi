using eKvarovi.Api.Data;
using eKvarovi.Api.Models;
using eKvarovi.Api.Services;
using eKvarovi.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly EKvaroviDbContext _db;
    private readonly TokenService _tokens;
    private readonly IPasswordHasher<AppUser> _hasher;

    public AuthController(
        EKvaroviDbContext db,
        TokenService tokens,
        IPasswordHasher<AppUser> hasher)
    {
        _db = db;
        _tokens = tokens;
        _hasher = hasher;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResultDto>> Login(LoginDto dto)
    {
        var username = dto.Username.Trim().ToLowerInvariant();

        var user = await _db.AppUsers
            .Include(u => u.Employee)
                .ThenInclude(e => e!.Location)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.AppRole)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username);

        if (user is null)
            return Unauthorized("Neispravno korisničko ime ili lozinka.");

        if (!user.IsActive)
            return Unauthorized("Korisnički račun je deaktiviran.");

        var verification = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

        if (verification == PasswordVerificationResult.Failed)
            return Unauthorized("Neispravno korisničko ime ili lozinka.");

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, dto.Password);
            await _db.SaveChangesAsync();
        }

        var roles = user.UserRoles.Select(ur => ur.AppRole!.Name).ToList();
        var (token, expiresAt) = _tokens.CreateToken(user, roles);

        return Ok(new LoginResultDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new CurrentUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                EmployeeId = user.EmployeeId,
                EmployeeName = user.Employee != null
                    ? user.Employee.FirstName + " " + user.Employee.LastName
                    : null,
                LocationName = user.Employee?.Location?.Name,
                Roles = roles
            }
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserDto>> Me()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await _db.AppUsers
            .Include(u => u.Employee)
                .ThenInclude(e => e!.Location)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.AppRole)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null || !user.IsActive)
            return Unauthorized("Račun više nije dostupan.");

        return Ok(new CurrentUserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            EmployeeId = user.EmployeeId,
            EmployeeName = user.Employee != null
                ? user.Employee.FirstName + " " + user.Employee.LastName
                : null,
            LocationName = user.Employee?.Location?.Name,
            Roles = user.UserRoles.Select(ur => ur.AppRole!.Name).ToList()
        });
    }
}