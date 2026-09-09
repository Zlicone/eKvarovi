using System.Net.Http.Json;
using eKvarovi.Shared.Dtos;

namespace eKvarovi.App.Services;

public class AttachmentService
{
    private readonly HttpClient _http;

    public AttachmentService(HttpClient http)
    {
        _http = http;
    }

    public string BaseUrl => _http.BaseAddress!.ToString();

    public async Task<List<AttachmentDto>> GetAllAsync(
        int? faultReportId = null,
        int? interventionId = null,
        int? purposeId = null)
    {
        var q = new List<string>();

        if (faultReportId.HasValue) q.Add($"faultReportId={faultReportId}");
        if (interventionId.HasValue) q.Add($"interventionId={interventionId}");
        if (purposeId.HasValue) q.Add($"purposeId={purposeId}");

        var url = "api/attachments";
        if (q.Count > 0) url += "?" + string.Join("&", q);

        var result = await _http.GetFromJsonAsync<List<AttachmentDto>>(url);
        return result ?? new List<AttachmentDto>();
    }

    public async Task<(bool Success, string? Error)> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        int faultReportId,
        int purposeId,
        int? interventionId = null)
    {
        using var content = new MultipartFormDataContent();

        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        content.Add(fileContent, "file", fileName);
        content.Add(new StringContent(faultReportId.ToString()), "faultReportId");
        content.Add(new StringContent(purposeId.ToString()), "purposeId");

        if (interventionId.HasValue)
            content.Add(new StringContent(interventionId.Value.ToString()), "interventionId");

        var response = await _http.PostAsync("api/attachments", content);

        if (response.IsSuccessStatusCode) return (true, null);

        var message = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(message) ? "Prijenos nije uspio." : message);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/attachments/{id}");
        if (response.IsSuccessStatusCode) return (true, null);

        var message = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(message) ? "Brisanje nije uspjelo." : message);
    }
}