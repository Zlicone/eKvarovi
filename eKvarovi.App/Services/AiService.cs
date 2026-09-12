using System.Net.Http.Json;
using eKvarovi.Shared.Dtos;

namespace eKvarovi.App.Services;

public class AiService
{
    private readonly HttpClient _http;

    public AiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> GetProviderAsync()
    {
        var response = await _http.GetAsync("api/ai/provider");
        if (!response.IsSuccessStatusCode) return "Mock";
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<(AiSuggestionResultDto? Result, string? Error)> SuggestReportAsync(string description)
    {
        var response = await _http.PostAsJsonAsync("api/ai/suggest-report",
            new AiSuggestionRequestDto { Description = description });

        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<AiSuggestionResultDto>(), null);

        var message = await response.Content.ReadAsStringAsync();
        return (null, string.IsNullOrWhiteSpace(message)
            ? "Prijedlog nije moguće dohvatiti."
            : message);
    }

    public async Task<(AiSummaryResultDto? Result, string? Error)> SummarizeAssignmentAsync(int assignmentId)
    {
        var response = await _http.GetAsync($"api/ai/assignments/{assignmentId}/summary");

        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<AiSummaryResultDto>(), null);

        var message = await response.Content.ReadAsStringAsync();
        return (null, string.IsNullOrWhiteSpace(message)
            ? "Sažetak nije moguće dohvatiti."
            : message);
    }
}