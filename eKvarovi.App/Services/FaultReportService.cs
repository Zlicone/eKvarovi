using System.Net.Http.Json;
using eKvarovi.Shared.Dtos;

namespace eKvarovi.App.Services;

public class FaultReportService
{
    private readonly HttpClient _http;

    public FaultReportService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<FaultReportListDto>> GetAllAsync(
        string? search = null,
        int? statusId = null,
        int? priorityId = null,
        int? faultTypeId = null,
        int? locationId = null,
        int? technicianId = null,
        bool? unassigned = null,
        bool? overdue = null,
        DateTime? from = null,
        DateTime? to = null,
        string? sortBy = null,
        string? sortDir = null)
    {
        var q = new List<string>();

        if (!string.IsNullOrWhiteSpace(search)) q.Add($"search={Uri.EscapeDataString(search)}");
        if (statusId.HasValue) q.Add($"statusId={statusId}");
        if (priorityId.HasValue) q.Add($"priorityId={priorityId}");
        if (faultTypeId.HasValue) q.Add($"faultTypeId={faultTypeId}");
        if (locationId.HasValue) q.Add($"locationId={locationId}");
        if (technicianId.HasValue) q.Add($"technicianId={technicianId}");
        if (unassigned == true) q.Add("unassigned=true");
        if (overdue == true) q.Add("overdue=true");
        if (from.HasValue) q.Add($"from={from.Value:o}");
        if (to.HasValue) q.Add($"to={to.Value:o}");
        if (!string.IsNullOrWhiteSpace(sortBy)) q.Add($"sortBy={sortBy}");
        if (!string.IsNullOrWhiteSpace(sortDir)) q.Add($"sortDir={sortDir}");

        var url = "api/faultreports";
        if (q.Count > 0) url += "?" + string.Join("&", q);

        var result = await _http.GetFromJsonAsync<List<FaultReportListDto>>(url);
        return result ?? new List<FaultReportListDto>();
    }

    public async Task<FaultReportListDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetAsync($"api/faultreports/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<FaultReportListDto>();
    }

    public async Task<(bool Success, string? Error)> CreateAsync(FaultReportCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/faultreports", dto);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Spremanje nije uspjelo."));
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, FaultReportUpdateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/faultreports/{id}", dto);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Izmjena nije uspjela."));
    }

    public async Task<(bool Success, string? Error)> ReviewAsync(int id, FaultReportReviewDto dto)
    {
        var response = await _http.PostAsJsonAsync($"api/faultreports/{id}/review", dto);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Pregled nije spremljen."));
    }

    public async Task<(bool Success, string? Error)> CloseAsync(int id)
    {
        var response = await _http.PostAsync($"api/faultreports/{id}/close", null);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Zatvaranje nije uspjelo."));
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/faultreports/{id}");
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Brisanje nije uspjelo."));
    }

    private static async Task<string> ReadError(HttpResponseMessage response, string fallback)
    {
        var message = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(message) ? fallback : message;
    }

    public async Task<FaultReportDetailDto?> GetDetailAsync(int id)
    {
        var response = await _http.GetAsync($"api/faultreports/{id}/detail");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<FaultReportDetailDto>();
    }

    public async Task<List<FaultReportEventDto>> GetEventsAsync(int id)
    {
        var result = await _http.GetFromJsonAsync<List<FaultReportEventDto>>($"api/faultreports/{id}/events");
        return result ?? new List<FaultReportEventDto>();
    }
}