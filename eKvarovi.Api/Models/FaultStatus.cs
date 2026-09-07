namespace eKvarovi.Api.Models;

public class FaultStatus
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Rank { get; set; }

    public ICollection<FaultReport> FaultReports { get; set; } = new List<FaultReport>();
}