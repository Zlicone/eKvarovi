using System.Security.Claims;
using eKvarovi.Api.Models;
using eKvarovi.Api.Services;

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

    public static void Log(
        EKvaroviDbContext db,
        ClaimsPrincipal user,
        int faultReportId,
        string eventType,
        string? oldValue = null,
        string? newValue = null)
    {
        Log(db, faultReportId, eventType, oldValue, newValue, user.GetUserId());
    }
}