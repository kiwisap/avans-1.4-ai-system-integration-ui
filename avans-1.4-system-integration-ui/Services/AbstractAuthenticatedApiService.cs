using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace avans_1._4_system_integration_ui.Services;

public abstract class AbstractAuthenticatedApiService(HttpClient httpClient, TokenStorageService tokenStorage, TokenRefreshService refreshService)
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    // Static so one refresh coordinates across every authenticated service.
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    protected Task<T?> GetAsync<T>(string url)
        => SendAsync<T>(() => new HttpRequestMessage(HttpMethod.Get, url));

    protected Task<T?> PostAsync<T>(string url, object body)
        => SendAsync<T>(() => new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        });

    private async Task<T?> SendAsync<T>(Func<HttpRequestMessage> requestFactory)
    {
        var token = await EnsureValidAccessTokenAsync();
        var response = await Send(requestFactory, token);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            var refreshed = await RefreshAccessTokenAsync();
            if (string.IsNullOrEmpty(refreshed))
                throw new UnauthorizedAccessException("Session expired. Please log in again.");

            response = await Send(requestFactory, refreshed); // rebuild request, no clone needed
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private Task<HttpResponseMessage> Send(Func<HttpRequestMessage> requestFactory, string? token)
    {
        var request = requestFactory();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return httpClient.SendAsync(request);
    }

    private async Task<string?> EnsureValidAccessTokenAsync()
    {
        var tokens = await tokenStorage.GetTokensAsync();
        if (tokens is null) return null;

        // 30s skew so we don't ship a token that expires mid-flight.
        if (tokens.ExpiresAt > DateTimeOffset.UtcNow.AddSeconds(30))
            return tokens.AccessToken;

        return await RefreshAccessTokenAsync();
    }

    private async Task<string?> RefreshAccessTokenAsync()
    {
        await RefreshLock.WaitAsync();
        try
        {
            // Someone may have refreshed while we waited for the lock — re-read first.
            var current = await tokenStorage.GetTokensAsync();
            if (current is null) return null;

            if (current.ExpiresAt > DateTimeOffset.UtcNow.AddSeconds(30))
                return current.AccessToken;

            if (string.IsNullOrEmpty(current.RefreshToken))
            {
                await tokenStorage.ClearAsync();
                return null;
            }

            var newTokens = await refreshService.RefreshAsync(current.RefreshToken);
            if (newTokens is null)
            {
                await tokenStorage.ClearAsync();
                return null;
            }

            await tokenStorage.SetTokensAsync(newTokens);
            return newTokens.AccessToken;
        }
        finally
        {
            RefreshLock.Release();
        }
    }
}