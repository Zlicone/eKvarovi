using System.Linq.Expressions;
using eKvarovi.Api.Data;
using eKvarovi.Api.Models;
using eKvarovi.Api.Services;
using eKvarovi.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkAssignmentsController : ControllerBase
{
    private readonly EKvaroviDbContext _db;

    public WorkAssignmentsController(EKvaroviDbContext db)
    {
        _db = db;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<AssignmentDto>>> GetMine([FromQuery] bool? activeOnly)
    {
        var employeeId = User.GetEmployeeId();

        if (employeeId is null)
            return BadRequest("Vaš račun nije povezan sa zaposlenikom pa nema dodijeljenih naloga.");

        var query = _db.FaultAssignments
            .Where(a => a.TechnicianId == employeeId.Value);

        if (activeOnly == true)
            query = query.Where(a => a.UnassignedAt == null);

        var items = await query
            .OrderByDescending(a => a.UnassignedAt == null)
            .ThenByDescending(a => a.AssignedAt)
            .Select(a => new AssignmentDto
            {
                Id = a.Id,
                FaultReportId = a.FaultReportId,
                FaultReportTitle = a.FaultReport!.Title,
                LocationName = a.FaultReport!.Location!.Name,
                PriorityName = a.FaultReport!.Priority != null ? a.FaultReport!.Priority.Name : null,
                PriorityRank = a.FaultReport!.Priority != null ? a.FaultReport!.Priority.Rank : (int?)null,
                ReportStatusCode = a.FaultReport!.Status!.Code,
                ReportStatusName = a.FaultReport!.Status!.Name,
                DueDate = a.FaultReport!.DueDate,
                TechnicianId = a.TechnicianId,
                TechnicianName = a.Technician!.FirstName + " " + a.Technician!.LastName,
                AssignedAt = a.AssignedAt,
                UnassignedAt = a.UnassignedAt,
                IsActive = a.UnassignedAt == null,
                Note = a.Note,
                InterventionCount = a.Interventions.Count,
                HasCompletedIntervention = a.Interventions
                    .Any(i => i.Status!.Code == InterventionStatusCodes.Completed)
            })
            .ToListAsync();

        return Ok(items);
    }
}