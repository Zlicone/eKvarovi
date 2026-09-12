namespace eKvarovi.Shared.Dtos;

public class DashboardDto
{
    public int OpenReports { get; set; }
    public int CriticalReports { get; set; }
    public int OverdueReports { get; set; }
    public int UnassignedReports { get; set; }
    public int ActiveInterventions { get; set; }
    public int ClosedThisMonth { get; set; }
    public double? AverageResolutionHours { get; set; }

    public List<DashboardCountDto> ByStatus { get; set; } = new();
    public List<DashboardCountDto> ByFaultType { get; set; } = new();
    public List<DashboardCountDto> ByLocation { get; set; } = new();

    public List<FaultReportListDto> LatestReports { get; set; } = new();

    public PersonalStatsDto? Personal { get; set; }
}

public class DashboardCountDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class PersonalStatsDto
{
    public int MyOpenReports { get; set; }
    public int MyResolvedReports { get; set; }
    public int MyActiveAssignments { get; set; }
    public int MyOpenInterventions { get; set; }
}