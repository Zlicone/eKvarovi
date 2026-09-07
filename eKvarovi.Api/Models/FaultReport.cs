namespace eKvarovi.Api.Models;

public class FaultReport
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int LocationId { get; set; }
    public Location? Location { get; set; }

    public int ReporterId { get; set; }
    public Employee? Reporter { get; set; }

    public int? FaultTypeId { get; set; }
    public FaultType? FaultType { get; set; }

    public int? PriorityId { get; set; }
    public FaultPriority? Priority { get; set; }

    public int StatusId { get; set; }
    public FaultStatus? Status { get; set; }

    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public int? ClosedByEmployeeId { get; set; }
    public Employee? ClosedByEmployee { get; set; }

    public ICollection<FaultAssignment> Assignments { get; set; } = new List<FaultAssignment>();
    public ICollection<FaultAttachment> Attachments { get; set; } = new List<FaultAttachment>();
    public ICollection<FaultReportEvent> Events { get; set; } = new List<FaultReportEvent>();
}