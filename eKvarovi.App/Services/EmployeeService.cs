using System.Net.Http.Json;
using eKvarovi.Shared.Dtos;

namespace eKvarovi.App.Services;

public class EmployeeService
{
    private readonly HttpClient _http;

    public EmployeeService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<EmployeeDto>> GetAllAsync(
        string? search = null,
        int? locationId = null,
        bool? isActive = null)
    {
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
            query.Add($"search={Uri.EscapeDataString(search)}");
        if (locationId.HasValue)
            query.Add($"locationId={locationId.Value}");
        if (isActive.HasValue)
            query.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");

        var url = "api/employees";
        if (query.Count > 0)
            url += "?" + string.Join("&", query);

        var result = await _http.GetFromJsonAsync<List<EmployeeDto>>(url);
        return result ?? new List<EmployeeDto>();
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetAsync($"api/employees/{id}");
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<EmployeeDto>();
    }

    public async Task<(bool Success, string? Error)> CreateAsync(EmployeeCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/employees", dto);
        if (response.IsSuccessStatusCode)
            return (true, null);

        var message = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(message) ? "Spremanje nije uspjelo." : message);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, EmployeeUpdateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/employees/{id}", dto);
        if (response.IsSuccessStatusCode)
            return (true, null);

        var message = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(message) ? "Izmjena nije uspjela." : message);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/employees/{id}");
        if (response.IsSuccessStatusCode)
            return (true, null);

        var message = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(message) ? "Brisanje nije uspjelo." : message);
    }
}