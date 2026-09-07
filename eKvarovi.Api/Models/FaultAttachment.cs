namespace eKvarovi.Api.Models;

public class FaultAttachment
{
    public int Id { get; set; }

    public int FaultReportId { get; set; }
    public FaultReport? FaultReport { get; set; }

    public int? InterventionId { get; set; }
    public Intervention? Intervention { get; set; }

    public int PurposeId { get; set; }
    public AttachmentPurpose? Purpose { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }

    public int? UploadedByAppUserId { get; set; }
    public AppUser? UploadedByAppUser { get; set; }
}