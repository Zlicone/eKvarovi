using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.Dtos;

public class InterventionDto
{
    public int Id { get; set; }

    public int FaultAssignmentId { get; set; }
    public int FaultReportId { get; set; }
    public string FaultReportTitle { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;

    public int StatusId { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;

    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }

    public int MaterialCount { get; set; }
    public bool IsEditable { get; set; }
}

public class InterventionMaterialDto
{
    public int Id { get; set; }
    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string UnitAbbreviation { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

public class InterventionCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Radni nalog je obavezan.")]
    public int FaultAssignmentId { get; set; }

    [StringLength(2000, ErrorMessage = "Bilješka smije imati najviše 2000 znakova.")]
    public string? Note { get; set; }
}

public class InterventionUpdateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Status je obavezan.")]
    public int StatusId { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    [StringLength(2000, ErrorMessage = "Bilješka smije imati najviše 2000 znakova.")]
    public string? Note { get; set; }
}

public class MaterialUsageDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Materijal je obavezan.")]
    public int MaterialId { get; set; }

    [Range(0.01, 100000, ErrorMessage = "Količina mora biti veća od nule.")]
    public decimal Quantity { get; set; }
}