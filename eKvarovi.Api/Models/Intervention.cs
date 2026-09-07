namespace eKvarovi.Api.Models;

public class Intervention
{
    public int Id { get; set; }

    public int FaultAssignmentId { get; set; }
    public FaultAssignment? FaultAssignment { get; set; }

    public int StatusId { get; set; }
    public InterventionStatus? Status { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<InterventionMaterial> Materials { get; set; } = new List<InterventionMaterial>();
    public ICollection<FaultAttachment> Attachments { get; set; } = new List<FaultAttachment>();
}