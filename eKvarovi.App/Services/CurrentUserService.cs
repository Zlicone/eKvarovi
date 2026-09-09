using System.Net.Http.Headers;
using eKvarovi.Shared.Dtos;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace eKvarovi.App.Services;

public class CurrentUserService
{
    private const string StorageKey = "ekvarovi.auth.session";

    private readonly HttpClient _http;
    private readonly ProtectedLocalStorage _storage;

    public CurrentUserService(HttpClient http, ProtectedLocalStorage storage)
    {
        _http = http;
        _storage = storage;
    }

    public CurrentUserDto? User { get; private set; }
    public string? Token { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsInitialized { get; private set; }

    public bool IsLoggedIn => User is not null && !IsExpired;

    private bool IsExpired => !ExpiresAt.HasValue || ExpiresAt.Value <= DateTime.UtcNow;

    public event Action? Changed;

    public async Task InitializeAsync()
    {
        if (IsInitialized) return;

        try
        {
            var stored = await _storage.GetAsync<LoginResultDto>(StorageKey);

            if (stored.Success &&
                stored.Value is not null &&
                !string.IsNullOrWhiteSpace(stored.Value.Token) &&
                stored.Value.ExpiresAt > DateTime.UtcNow.AddSeconds(30))
            {
                Apply(stored.Value);
            }
            else
            {
                await ClearAsync();
            }
        }
        catch
        {
            await ClearAsync();
        }
        finally
        {
            IsInitialized = true;
            Changed?.Invoke();
        }
    }

    public async Task LoginAsync(LoginResultDto result)
    {
        Apply(result);
        await _storage.SetAsync(StorageKey, result);
        Changed?.Invoke();
    }

    public async Task LogoutAsync()
    {
        await ClearAsync();
        Changed?.Invoke();
    }

    public bool HasRole(string role) => User?.Roles.Contains(role) == true;

    public bool HasAnyRole(params string[] roles) => User?.Roles.Any(roles.Contains) == true;

    private void Apply(LoginResultDto result)
    {
        User = result.User;
        Token = result.Token;
        ExpiresAt = result.ExpiresAt;

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", result.Token);
    }

    private async Task ClearAsync()
    {
        User = null;
        Token = null;
        ExpiresAt = null;
        _http.DefaultRequestHeaders.Authorization = null;

        try
        {
            await _storage.DeleteAsync(StorageKey);
        }
        catch
        {
            // brisanje pri prerenderu nije moguće, nije problem
        }
    }
}