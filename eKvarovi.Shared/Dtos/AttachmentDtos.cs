namespace eKvarovi.Shared.Dtos;

public class AttachmentDto
{
    public int Id { get; set; }
    public int FaultReportId { get; set; }
    public int? InterventionId { get; set; }

    public int PurposeId { get; set; }
    public string PurposeCode { get; set; } = string.Empty;
    public string PurposeName { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }

    public bool IsImage => ContentType.StartsWith("image/");
    public string Url => $"uploads/{StoredFileName}";
}