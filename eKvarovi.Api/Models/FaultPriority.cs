namespace eKvarovi.Api.Models;

public class FaultPriority
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Rank { get; set; }

    public ICollection<FaultReport> FaultReports { get; set; } = new List<FaultReport>();
}