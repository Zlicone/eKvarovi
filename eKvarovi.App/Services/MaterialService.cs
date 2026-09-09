using System.Net.Http.Json;
using eKvarovi.Shared.Dtos;

namespace eKvarovi.App.Services;

public class MaterialService
{
    private readonly HttpClient _http;

    public MaterialService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<MaterialDto>> GetAllAsync(
        string? search = null,
        int? unitId = null,
        bool? isActive = null)
    {
        var q = new List<string>();

        if (!string.IsNullOrWhiteSpace(search)) q.Add($"search={Uri.EscapeDataString(search)}");
        if (unitId.HasValue) q.Add($"unitId={unitId}");
        if (isActive.HasValue) q.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");

        var url = "api/materials";
        if (q.Count > 0) url += "?" + string.Join("&", q);

        var result = await _http.GetFromJsonAsync<List<MaterialDto>>(url);
        return result ?? new List<MaterialDto>();
    }

    public async Task<(bool Success, string? Error)> CreateAsync(MaterialCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/materials", dto);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Spremanje nije uspjelo."));
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, MaterialUpdateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/materials/{id}", dto);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Izmjena nije uspjela."));
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/materials/{id}");
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Brisanje nije uspjelo."));
    }

    private static async Task<string> ReadError(HttpResponseMessage response, string fallback)
    {
        var message = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(message) ? fallback : message;
    }
}