using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.Dtos;

public class LoginDto
{
    [Required(ErrorMessage = "Korisničko ime je obavezno.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lozinka je obavezna.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginResultDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public CurrentUserDto User { get; set; } = new();
}

public class CurrentUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? LocationName { get; set; }
    public List<string> Roles { get; set; } = new();

    public bool IsAdmin => Roles.Contains("Admin");
    public bool IsManager => Roles.Contains("Manager");
    public bool IsTechnician => Roles.Contains("Technician");
    public bool IsReporter => Roles.Contains("Reporter");
}

public class AppUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class AppUserCreateDto
{
    [Required(ErrorMessage = "Korisničko ime je obavezno.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Korisničko ime mora imati između 3 i 100 znakova.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email je obavezan.")]
    [EmailAddress(ErrorMessage = "Email nije u ispravnom obliku.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lozinka je obavezna.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Lozinka mora imati najmanje 6 znakova.")]
    public string Password { get; set; } = string.Empty;

    public int? EmployeeId { get; set; }

    [MinLength(1, ErrorMessage = "Potrebno je odabrati barem jednu ulogu.")]
    public List<int> RoleIds { get; set; } = new();
}

public class AppUserUpdateDto
{
    [Required(ErrorMessage = "Email je obavezan.")]
    [EmailAddress(ErrorMessage = "Email nije u ispravnom obliku.")]
    public string Email { get; set; } = string.Empty;

    public int? EmployeeId { get; set; }
    public bool IsActive { get; set; }

    [MinLength(1, ErrorMessage = "Potrebno je odabrati barem jednu ulogu.")]
    public List<int> RoleIds { get; set; } = new();
}

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Nova lozinka je obavezna.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Lozinka mora imati najmanje 6 znakova.")]
    public string NewPassword { get; set; } = string.Empty;
}