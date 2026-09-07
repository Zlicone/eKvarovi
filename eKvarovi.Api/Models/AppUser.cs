namespace eKvarovi.Api.Models;

public class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}