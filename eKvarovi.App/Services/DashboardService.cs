using System.Net.Http.Json;
using eKvarovi.Shared.Dtos;

namespace eKvarovi.App.Services;

public class DashboardService
{
    private readonly HttpClient _http;

    public DashboardService(HttpClient http)
    {
        _http = http;
    }

    public async Task<DashboardDto?> GetAsync()
    {
        var response = await _http.GetAsync("api/dashboard");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<DashboardDto>();
    }

    public async Task<SlaOverviewDto?> GetSlaAsync(int months = 6)
    {
        var response = await _http.GetAsync($"api/dashboard/sla?months={months}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SlaOverviewDto>();
    }
}