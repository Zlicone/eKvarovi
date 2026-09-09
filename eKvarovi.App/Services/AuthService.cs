using System.Net.Http.Json;
using eKvarovi.Shared.Dtos;

namespace eKvarovi.App.Services;

public class AuthService
{
    private readonly HttpClient _http;

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public async Task<(LoginResultDto? Result, string? Error)> LoginAsync(LoginDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", dto);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResultDto>();
            return (result, null);
        }

        var message = await response.Content.ReadAsStringAsync();
        return (null, string.IsNullOrWhiteSpace(message)
            ? "Prijava nije uspjela."
            : message);
    }

    public async Task<CurrentUserDto?> GetMeAsync()
    {
        var response = await _http.GetAsync("api/auth/me");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CurrentUserDto>();
    }
}