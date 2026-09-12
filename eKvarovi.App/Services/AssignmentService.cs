using System.Net.Http.Json;
using eKvarovi.Shared.Dtos;

namespace eKvarovi.App.Services;

public class AssignmentService
{
    private readonly HttpClient _http;

    public AssignmentService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<AssignmentDto>> GetAllAsync(
        int? faultReportId = null,
        int? technicianId = null,
        bool? activeOnly = null,
        string? search = null)
    {
        var q = new List<string>();

        if (faultReportId.HasValue) q.Add($"faultReportId={faultReportId}");
        if (technicianId.HasValue) q.Add($"technicianId={technicianId}");
        if (activeOnly == true) q.Add("activeOnly=true");
        if (!string.IsNullOrWhiteSpace(search)) q.Add($"search={Uri.EscapeDataString(search)}");

        var url = "api/assignments";
        if (q.Count > 0) url += "?" + string.Join("&", q);

        var result = await _http.GetFromJsonAsync<List<AssignmentDto>>(url);
        return result ?? new List<AssignmentDto>();
    }

    public async Task<List<AssignmentDto>> GetMineAsync(bool? activeOnly = null)
    {
        var url = "api/workassignments/mine";

        if (activeOnly == true)
            url += "?activeOnly=true";

        var result = await _http.GetFromJsonAsync<List<AssignmentDto>>(url);
        return result ?? new List<AssignmentDto>();
    }

    public async Task<(bool Success, string? Error)> CreateAsync(AssignmentCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/assignments", dto);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Dodjela nije uspjela."));
    }

    public async Task<(bool Success, string? Error)> UnassignAsync(int id)
    {
        var response = await _http.PostAsync($"api/assignments/{id}/unassign", null);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Skidanje naloga nije uspjelo."));
    }

    private static async Task<string> ReadError(HttpResponseMessage response, string fallback)
    {
        var message = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(message) ? fallback : message;
    }
}