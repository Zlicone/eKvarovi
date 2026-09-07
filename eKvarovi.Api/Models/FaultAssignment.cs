namespace eKvarovi.Api.Models;

public class FaultAssignment
{
    public int Id { get; set; }

    public int FaultReportId { get; set; }
    public FaultReport? FaultReport { get; set; }

    public int TechnicianId { get; set; }
    public Employee? Technician { get; set; }

    public int? AssignedByEmployeeId { get; set; }
    public Employee? AssignedByEmployee { get; set; }

    public DateTime AssignedAt { get; set; }
    public DateTime? UnassignedAt { get; set; }
    public string? Note { get; set; }

    public ICollection<Intervention> Interventions { get; set; } = new List<Intervention>();
}