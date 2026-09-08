using System.Net.Http.Json;
using eKvarovi.Shared.Dtos;

namespace eKvarovi.App.Services;

public class LookupService
{
    private readonly HttpClient _http;

    public LookupService(HttpClient http)
    {
        _http = http;
    }

    public Task<List<LookupDto>> GetLocationTypesAsync() => GetAsync("location-types");
    public Task<List<LookupDto>> GetFaultTypesAsync() => GetAsync("fault-types");
    public Task<List<LookupDto>> GetFaultPrioritiesAsync() => GetAsync("fault-priorities");
    public Task<List<LookupDto>> GetFaultStatusesAsync() => GetAsync("fault-statuses");
    public Task<List<LookupDto>> GetInterventionStatusesAsync() => GetAsync("intervention-statuses");
    public Task<List<LookupDto>> GetMaterialUnitsAsync() => GetAsync("material-units");
    public Task<List<LookupDto>> GetAttachmentPurposesAsync() => GetAsync("attachment-purposes");

    private async Task<List<LookupDto>> GetAsync(string endpoint)
    {
        var result = await _http.GetFromJsonAsync<List<LookupDto>>($"api/lookups/{endpoint}");
        return result ?? new List<LookupDto>();
    }
}