using System.Text.Json;
using Microsoft.JSInterop;
using avans_1._4_system_integration_ui.Models.Dto;
using avans_1._4_system_integration_ui.Models;

namespace avans_1._4_system_integration_ui.Services;

public class TokenStorageService(IJSRuntime jsRuntime)
{
    private const string StorageKey = "authTokens";

    public async Task<AuthTokens?> GetTokensAsync()
    {
        try
        {
            var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<AuthTokens>(json);
        }
        catch
        {
            return null; // prerender / interop not available yet
        }
    }

    public async Task SetTokensAsync(AccessTokenResponseDto tokens)
    {
        var stored = new AuthTokens(
            tokens.AccessToken,
            tokens.RefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(tokens.ExpiresIn));

        await jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, JsonSerializer.Serialize(stored));
    }

    public async Task ClearAsync()
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
    }
}