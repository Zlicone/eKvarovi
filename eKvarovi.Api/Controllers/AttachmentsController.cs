using eKvarovi.Api.Data;
using eKvarovi.Api.Models;
using eKvarovi.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    private static readonly Dictionary<string, string[]> AllowedTypes = new()
    {
        [".jpg"] = new[] { "image/jpeg" },
        [".jpeg"] = new[] { "image/jpeg" },
        [".png"] = new[] { "image/png" },
        [".webp"] = new[] { "image/webp" },
        [".pdf"] = new[] { "application/pdf" }
    };

    private readonly EKvaroviDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AttachmentsController(EKvaroviDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet]
    public async Task<ActionResult<List<AttachmentDto>>> GetAll(
        [FromQuery] int? faultReportId,
        [FromQuery] int? interventionId,
        [FromQuery] int? purposeId)
    {
        var query = _db.FaultAttachments.AsQueryable();

        if (faultReportId.HasValue)
            query = query.Where(a => a.FaultReportId == faultReportId.Value);

        if (interventionId.HasValue)
            query = query.Where(a => a.InterventionId == interventionId.Value);

        if (purposeId.HasValue)
            query = query.Where(a => a.PurposeId == purposeId.Value);

        var items = await query
            .OrderBy(a => a.PurposeId)
            .ThenByDescending(a => a.UploadedAt)
            .Select(a => new AttachmentDto
            {
                Id = a.Id,
                FaultReportId = a.FaultReportId,
                InterventionId = a.InterventionId,
                PurposeId = a.PurposeId,
                PurposeCode = a.Purpose!.Code,
                PurposeName = a.Purpose!.Name,
                OriginalFileName = a.OriginalFileName,
                StoredFileName = a.StoredFileName,
                ContentType = a.ContentType,
                SizeBytes = a.SizeBytes,
                UploadedAt = a.UploadedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    [RequestSizeLimit(MaxFileSize + 1024)]
    [Authorize(Policy = "Fieldwork")]
    public async Task<ActionResult> Upload(
        [FromForm] AttachmentUploadForm form)
    {
        var file = form.File;
        var faultReportId = form.FaultReportId;
        var purposeId = form.PurposeId;
        var interventionId = form.InterventionId;

        if (file is null || file.Length == 0)
            return BadRequest("Datoteka nije poslana ili je prazna.");

        if (file.Length > MaxFileSize)
            return BadRequest("Datoteka je prevelika. Najveća dopuštena veličina je 5 MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedTypes.TryGetValue(extension, out var allowedContentTypes))
            return BadRequest("Dopušteni su samo formati JPG, PNG, WEBP i PDF.");

        if (!allowedContentTypes.Contains(file.ContentType))
            return BadRequest("Sadržaj datoteke ne odgovara njezinoj ekstenziji.");

        var report = await _db.FaultReports
            .Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.Id == faultReportId);

        if (report is null)
            return BadRequest("Odabrana prijava ne postoji.");

        if (report.Status!.Code == FaultStatusCodes.Closed)
            return BadRequest("Privitke nije moguće dodavati na zatvorenu prijavu.");

        if (!await _db.AttachmentPurposes.AnyAsync(p => p.Id == purposeId))
            return BadRequest("Odabrana namjena privitka ne postoji.");

        if (interventionId.HasValue)
        {
            var belongs = await _db.Interventions.AnyAsync(i =>
                i.Id == interventionId.Value &&
                i.FaultAssignment!.FaultReportId == faultReportId);

            if (!belongs)
                return BadRequest("Odabrana intervencija ne pripada toj prijavi.");
        }

        var uploadsPath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsPath);

        var storedName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsPath, storedName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        _db.FaultAttachments.Add(new FaultAttachment
        {
            FaultReportId = faultReportId,
            InterventionId = interventionId,
            PurposeId = purposeId,
            OriginalFileName = Path.GetFileName(file.FileName),
            StoredFileName = storedName,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            UploadedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Fieldwork")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.FaultAttachments
            .Include(a => a.FaultReport)
                .ThenInclude(r => r!.Status)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (entity is null)
            return NotFound($"Privitak s ID-em {id} ne postoji.");

        if (entity.FaultReport!.Status!.Code == FaultStatusCodes.Closed)
            return BadRequest("Privitke nije moguće brisati sa zatvorene prijave.");

        var fullPath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", entity.StoredFileName);

        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);

        _db.FaultAttachments.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public class AttachmentUploadForm
{
    public IFormFile File { get; set; } = default!;
    public int FaultReportId { get; set; }
    public int PurposeId { get; set; }
    public int? InterventionId { get; set; }
}