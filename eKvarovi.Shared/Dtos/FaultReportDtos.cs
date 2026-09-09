using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.Dtos;

public class FaultReportListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;

    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;

    public string ReporterName { get; set; } = string.Empty;

    public int? FaultTypeId { get; set; }
    public string? FaultTypeName { get; set; }

    public int? PriorityId { get; set; }
    public string? PriorityName { get; set; }
    public int? PriorityRank { get; set; }

    public int StatusId { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? ActiveTechnicianName { get; set; }
    public bool IsOverdue { get; set; }
    public int InterventionCount { get; set; }
    public int AttachmentCount { get; set; }
}

public class FaultReportCreateDto
{
    [Required(ErrorMessage = "Naslov je obavezan.")]
    [StringLength(200, ErrorMessage = "Naslov smije imati najviše 200 znakova.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Opis je obavezan.")]
    [StringLength(2000, ErrorMessage = "Opis smije imati najviše 2000 znakova.")]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Lokacija je obavezna.")]
    public int LocationId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Prijavitelj je obavezan.")]
    public int ReporterId { get; set; }
}

public class FaultReportUpdateDto
{
    [Required(ErrorMessage = "Naslov je obavezan.")]
    [StringLength(200, ErrorMessage = "Naslov smije imati najviše 200 znakova.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Opis je obavezan.")]
    [StringLength(2000, ErrorMessage = "Opis smije imati najviše 2000 znakova.")]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Lokacija je obavezna.")]
    public int LocationId { get; set; }
}

public class FaultReportReviewDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Vrsta kvara je obavezna.")]
    public int FaultTypeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Prioritet je obavezan.")]
    public int PriorityId { get; set; }

    public DateTime? DueDate { get; set; }
}

public class FaultReportDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;
    public string LocationCity { get; set; } = string.Empty;

    public int ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string ReporterEmail { get; set; } = string.Empty;

    public int? FaultTypeId { get; set; }
    public string? FaultTypeName { get; set; }

    public int? PriorityId { get; set; }
    public string? PriorityName { get; set; }
    public int? PriorityRank { get; set; }

    public int StatusId { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ClosedByName { get; set; }

    public string? ActiveTechnicianName { get; set; }
    public bool IsOverdue { get; set; }
    public bool CanBeClosed { get; set; }
}

public class FaultReportEventDto
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedAt { get; set; }
}