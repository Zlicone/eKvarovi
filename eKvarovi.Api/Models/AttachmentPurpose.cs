namespace eKvarovi.Api.Models;

public class AttachmentPurpose
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<FaultAttachment> Attachments { get; set; } = new List<FaultAttachment>();
}