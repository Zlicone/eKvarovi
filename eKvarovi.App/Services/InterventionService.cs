using System.Net.Http.Json;
using eKvarovi.Shared.Dtos;

namespace eKvarovi.App.Services;

public class InterventionService
{
    private readonly HttpClient _http;

    public InterventionService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<InterventionDto>> GetAllAsync(
        int? faultReportId = null,
        int? faultAssignmentId = null,
        int? technicianId = null,
        int? statusId = null,
        string? search = null)
    {
        var q = new List<string>();

        if (faultReportId.HasValue) q.Add($"faultReportId={faultReportId}");
        if (faultAssignmentId.HasValue) q.Add($"faultAssignmentId={faultAssignmentId}");
        if (technicianId.HasValue) q.Add($"technicianId={technicianId}");
        if (statusId.HasValue) q.Add($"statusId={statusId}");
        if (!string.IsNullOrWhiteSpace(search)) q.Add($"search={Uri.EscapeDataString(search)}");

        var url = "api/interventions";
        if (q.Count > 0) url += "?" + string.Join("&", q);

        var result = await _http.GetFromJsonAsync<List<InterventionDto>>(url);
        return result ?? new List<InterventionDto>();
    }

    public async Task<List<InterventionMaterialDto>> GetMaterialsAsync(int interventionId)
    {
        var result = await _http.GetFromJsonAsync<List<InterventionMaterialDto>>(
            $"api/interventions/{interventionId}/materials");
        return result ?? new List<InterventionMaterialDto>();
    }

    public async Task<(bool Success, string? Error)> CreateAsync(InterventionCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/interventions", dto);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Otvaranje intervencije nije uspjelo."));
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, InterventionUpdateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/interventions/{id}", dto);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Spremanje nije uspjelo."));
    }

    public async Task<(bool Success, string? Error)> AddMaterialAsync(int id, MaterialUsageDto dto)
    {
        var response = await _http.PostAsJsonAsync($"api/interventions/{id}/materials", dto);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Dodavanje materijala nije uspjelo."));
    }

    public async Task<(bool Success, string? Error)> RemoveMaterialAsync(int id, int materialId)
    {
        var response = await _http.DeleteAsync($"api/interventions/{id}/materials/{materialId}");
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Uklanjanje materijala nije uspjelo."));
    }

    private static async Task<string> ReadError(HttpResponseMessage response, string fallback)
    {
        var message = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(message) ? fallback : message;
    }
}