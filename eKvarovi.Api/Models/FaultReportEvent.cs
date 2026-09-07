namespace eKvarovi.Api.Models;

public class FaultReportEvent
{
    public int Id { get; set; }

    public int FaultReportId { get; set; }
    public FaultReport? FaultReport { get; set; }

    public string EventType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }

    public int? ChangedByAppUserId { get; set; }
    public AppUser? ChangedByAppUser { get; set; }

    public DateTime ChangedAt { get; set; }
}