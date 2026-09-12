using System.Net.Http.Json;
using eKvarovi.Shared.Dtos;

namespace eKvarovi.App.Services;

public class UserService
{
    private readonly HttpClient _http;

    public UserService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<AppUserDto>> GetAllAsync(string? search = null, bool? isActive = null)
    {
        var q = new List<string>();

        if (!string.IsNullOrWhiteSpace(search)) q.Add($"search={Uri.EscapeDataString(search)}");
        if (isActive.HasValue) q.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");

        var url = "api/users";
        if (q.Count > 0) url += "?" + string.Join("&", q);

        var result = await _http.GetFromJsonAsync<List<AppUserDto>>(url);
        return result ?? new List<AppUserDto>();
    }

    public async Task<List<LookupDto>> GetRolesAsync()
    {
        var result = await _http.GetFromJsonAsync<List<LookupDto>>("api/users/roles");
        return result ?? new List<LookupDto>();
    }

    public async Task<(bool Success, string? Error)> CreateAsync(AppUserCreateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/users", dto);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Spremanje nije uspjelo."));
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, AppUserUpdateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/users/{id}", dto);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Izmjena nije uspjela."));
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(int id, ChangePasswordDto dto)
    {
        var response = await _http.PostAsJsonAsync($"api/users/{id}/password", dto);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Promjena lozinke nije uspjela."));
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/users/{id}");
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ReadError(response, "Brisanje nije uspjelo."));
    }

    private static async Task<string> ReadError(HttpResponseMessage response, string fallback)
    {
        var message = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(message) ? fallback : message;
    }
}