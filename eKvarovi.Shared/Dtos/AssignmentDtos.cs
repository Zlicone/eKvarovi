using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.Dtos;

public class AssignmentDto
{
    public int Id { get; set; }

    public int FaultReportId { get; set; }
    public string FaultReportTitle { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string? PriorityName { get; set; }
    public int? PriorityRank { get; set; }
    public string ReportStatusCode { get; set; } = string.Empty;
    public string ReportStatusName { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }

    public int TechnicianId { get; set; }
    public string TechnicianName { get; set; } = string.Empty;

    public string? AssignedByName { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? UnassignedAt { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }

    public int InterventionCount { get; set; }
    public bool HasCompletedIntervention { get; set; }
}

public class AssignmentCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Prijava je obavezna.")]
    public int FaultReportId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Izvršitelj je obavezan.")]
    public int TechnicianId { get; set; }

    public int? AssignedByEmployeeId { get; set; }

    [StringLength(500, ErrorMessage = "Napomena smije imati najviše 500 znakova.")]
    public string? Note { get; set; }
}