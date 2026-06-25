using avans_1._4_system_integration_ui.Models;
using avans_1._4_system_integration_ui.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace avans_1._4_system_integration_ui.Services;

public class AuthService(
    HttpClient httpClient,
    TokenStorageService tokenStorage,
    TokenRefreshService refreshService)
    : AbstractAuthenticatedApiService(httpClient, tokenStorage, refreshService)
{
    public async Task<AuthResponse> LoginAsync(LoginRequestDto request)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/identity/login", request);

            if (!response.IsSuccessStatusCode)
                return new AuthResponse { Success = false, Message = await ParseProblemDetails(response) };

            var tokens = await response.Content.ReadFromJsonAsync<AccessTokenResponseDto>();

            if (tokens is null || string.IsNullOrEmpty(tokens.AccessToken))
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid response from server - no token received."
                };

            await tokenStorage.SetTokensAsync(tokens);
            return new AuthResponse
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse
            {
                Success = false,
                Message = $"Exception during login: {ex.Message}"
            };
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/auth/register", request);

            return response.IsSuccessStatusCode
                ? new AuthResponse { Success = true }
                : new AuthResponse { Success = false, Message = await ParseProblemDetails(response) };
        }
        catch (Exception ex)
        {
            return new AuthResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        if (!await IsAuthenticatedAsync())
            return null;

        try
        {
            return await GetAsync<UserDto>("api/auth/me");
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    public Task LogoutAsync() => tokenStorage.ClearAsync();

    public async Task<bool> IsAuthenticatedAsync()
    {
        var tokens = await tokenStorage.GetTokensAsync();
        return tokens is not null && !string.IsNullOrEmpty(tokens.AccessToken);
    }

    private static async Task<string> ParseProblemDetails(HttpResponseMessage response)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            if (problem is not null)
                return problem.Detail ?? problem.Title ?? $"Error: {response.StatusCode}";
        }
        catch { /* fall through */ }

        return $"Error: {response.StatusCode}";
    }
}