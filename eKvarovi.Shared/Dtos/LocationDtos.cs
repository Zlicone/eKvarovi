using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.Dtos;

public class LocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int LocationTypeId { get; set; }
    public string LocationTypeName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int EmployeeCount { get; set; }
    public int FaultReportCount { get; set; }
}

public class LocationCreateDto
{
    [Required(ErrorMessage = "Naziv je obavezan.")]
    [StringLength(150, ErrorMessage = "Naziv smije imati najviše 150 znakova.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adresa je obavezna.")]
    [StringLength(200, ErrorMessage = "Adresa smije imati najviše 200 znakova.")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Grad je obavezan.")]
    [StringLength(100, ErrorMessage = "Grad smije imati najviše 100 znakova.")]
    public string City { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Vrsta lokacije je obavezna.")]
    public int LocationTypeId { get; set; }
}

public class LocationUpdateDto : LocationCreateDto
{
    public bool IsActive { get; set; }
}