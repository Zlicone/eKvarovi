using eKvarovi.Api.Models;

namespace eKvarovi.Api.Data;

public static class EventLogger
{
    public static void Log(
        EKvaroviDbContext db,
        int faultReportId,
        string eventType,
        string? oldValue = null,
        string? newValue = null,
        int? appUserId = null)
    {
        db.FaultReportEvents.Add(new FaultReportEvent
        {
            FaultReportId = faultReportId,
            EventType = eventType,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedByAppUserId = appUserId,
            ChangedAt = DateTime.UtcNow
        });
    }
}