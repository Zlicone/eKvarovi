using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.Dtos;

public class EmployeeDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool HasAccount { get; set; }
    public int ReportedCount { get; set; }
    public int ActiveAssignmentCount { get; set; }
}

public class EmployeeCreateDto
{
    [Required(ErrorMessage = "Ime je obavezno.")]
    [StringLength(100, ErrorMessage = "Ime smije imati najviše 100 znakova.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Prezime je obavezno.")]
    [StringLength(100, ErrorMessage = "Prezime smije imati najviše 100 znakova.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email je obavezan.")]
    [EmailAddress(ErrorMessage = "Email nije u ispravnom obliku.")]
    [StringLength(200, ErrorMessage = "Email smije imati najviše 200 znakova.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Telefon smije imati najviše 50 znakova.")]
    public string? Phone { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Lokacija je obavezna.")]
    public int LocationId { get; set; }
}

public class EmployeeUpdateDto : EmployeeCreateDto
{
    public bool IsActive { get; set; }
}