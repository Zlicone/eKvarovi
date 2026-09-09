using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.Dtos;

public class MaterialDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string UnitAbbreviation { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int UsageCount { get; set; }
}

public class MaterialCreateDto
{
    [Required(ErrorMessage = "Naziv je obavezan.")]
    [StringLength(150, ErrorMessage = "Naziv smije imati najviše 150 znakova.")]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Mjerna jedinica je obavezna.")]
    public int UnitId { get; set; }
}

public class MaterialUpdateDto : MaterialCreateDto
{
    public bool IsActive { get; set; }
}