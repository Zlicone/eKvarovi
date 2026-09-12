using eKvarovi.Api.Data;
using eKvarovi.Api.Services;
using eKvarovi.Api.Services.Ai;
using eKvarovi.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiService _ai;
    private readonly EKvaroviDbContext _db;

    public AiController(IAiService ai, EKvaroviDbContext db)
    {
        _ai = ai;
        _db = db;
    }

    [HttpGet("provider")]
    public ActionResult<string> GetProvider() => Ok(_ai.ProviderName);

    [HttpPost("suggest-report")]
    public async Task<ActionResult<AiSuggestionResultDto>> SuggestReport(
        AiSuggestionRequestDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _ai.SuggestReportAsync(dto.Description, cancellationToken);
        return Ok(result);
    }

    [HttpGet("assignments/{id:int}/summary")]
    public async Task<ActionResult<AiSummaryResultDto>> SummarizeAssignment(
        int id,
        CancellationToken cancellationToken)
    {
        var assignment = await _db.FaultAssignments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (assignment is null)
            return NotFound($"Radni nalog s ID-em {id} ne postoji.");

        var employeeId = User.GetEmployeeId();
        var isManager = User.IsInAnyRole(RoleNames.Admin, RoleNames.Manager);

        if (!isManager && assignment.TechnicianId != employeeId)
            return Forbid();

        var result = await _ai.SummarizeAssignmentAsync(id, cancellationToken);
        return Ok(result);
    }
}