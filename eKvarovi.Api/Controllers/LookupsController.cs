using eKvarovi.Api.Data;
using eKvarovi.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly EKvaroviDbContext _db;

    public LookupsController(EKvaroviDbContext db)
    {
        _db = db;
    }

    [HttpGet("location-types")]
    public async Task<ActionResult<List<LookupDto>>> GetLocationTypes()
    {
        var items = await _db.LocationTypes
            .OrderBy(x => x.Name)
            .Select(x => new LookupDto { Id = x.Id, Name = x.Name })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("fault-types")]
    public async Task<ActionResult<List<LookupDto>>> GetFaultTypes()
    {
        var items = await _db.FaultTypes
            .OrderBy(x => x.Name)
            .Select(x => new LookupDto { Id = x.Id, Name = x.Name })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("fault-priorities")]
    public async Task<ActionResult<List<LookupDto>>> GetFaultPriorities()
    {
        var items = await _db.FaultPriorities
            .OrderBy(x => x.Rank)
            .Select(x => new LookupDto { Id = x.Id, Name = x.Name })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("fault-statuses")]
    public async Task<ActionResult<List<LookupDto>>> GetFaultStatuses()
    {
        var items = await _db.FaultStatuses
            .OrderBy(x => x.Rank)
            .Select(x => new LookupDto { Id = x.Id, Name = x.Name })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("intervention-statuses")]
    public async Task<ActionResult<List<LookupDto>>> GetInterventionStatuses()
    {
        var items = await _db.InterventionStatuses
            .OrderBy(x => x.Id)
            .Select(x => new LookupDto { Id = x.Id, Name = x.Name })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("material-units")]
    public async Task<ActionResult<List<LookupDto>>> GetMaterialUnits()
    {
        var items = await _db.MaterialUnits
            .OrderBy(x => x.Name)
            .Select(x => new LookupDto { Id = x.Id, Name = x.Name })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("attachment-purposes")]
    public async Task<ActionResult<List<LookupDto>>> GetAttachmentPurposes()
    {
        var items = await _db.AttachmentPurposes
            .OrderBy(x => x.Id)
            .Select(x => new LookupDto { Id = x.Id, Name = x.Name })
            .ToListAsync();

        return Ok(items);
    }
}