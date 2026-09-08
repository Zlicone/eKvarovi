using System.Net.Http.Json;
using eKvarovi.Shared.Dtos;

namespace eKvarovi.App.Services;

public class LocationService
{
    private readonly HttpClient _http;

    public LocationService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<LocationDto>> GetAllAsync(
        string? search = null,
        int? locationTypeId = null,
        bool? isActive = null,
        string? sortBy = null,
        string? sortDir = null)
    {
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
            query.Add($"search={Uri.EscapeDataString(search)}");
        if (locationTypeId.HasValue)
            query.Add($"locationTypeId={locationTypeId.Value}");
        if (isActive.HasValue)
            query.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");
        if (!string.IsNullOrWhiteSpace(sortBy))
            query.Add($"sortBy={sortBy}");
        if (!string.IsNullOrWhiteSpace(sortDir))
            query.Add($"sortDir={sortDir}");

        var url = "api/locations";
        if (query.Count > 0)
            url += "?" + string.Join("&", query);

        var result = await _http.GetFromJsonAsync<List<LocationDto>>(url);
        return result ?? new List<LocationDto>();
    }

    public async Task<(bool Success, string? Error)> CreateAsync(LocationCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/locations", dto);
        if (response.IsSuccessStatusCode)
            return (true, null);

        var message = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(message) ? "Spremanje nije uspjelo." : message);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, LocationUpdateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/locations/{id}", dto);
        if (response.IsSuccessStatusCode)
            return (true, null);

        var message = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(message) ? "Izmjena nije uspjela." : message);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/locations/{id}");
        if (response.IsSuccessStatusCode)
            return (true, null);

        var message = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(message) ? "Brisanje nije uspjelo." : message);
    }
}