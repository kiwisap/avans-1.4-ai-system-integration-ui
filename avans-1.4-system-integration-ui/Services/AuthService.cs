using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using avans_1._4_system_integration_ui.Models;

namespace avans_1._4_system_integration_ui.Services;

public class AuthService(HttpClient httpClient, TokenStorageService tokenStorage)
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<AuthResponse> LoginAsync(LoginRequestDto request)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/identity/login", request);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResponseDto>(responseBody, jsonOptions);

                var token = result?.AccessToken ?? result?.Token;

                if (!string.IsNullOrEmpty(token))
                {
                    await tokenStorage.SetTokenAsync(token);
                    return new AuthResponse { Success = true };
                }

                return new AuthResponse { Success = false, Message = "Invalid response from server - no token received." };
            }
            else
            {
                var errorMessage = await ParseProblemDetails(response);
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = errorMessage
                };
            }
        }
        catch (Exception ex)
        {
            return new AuthResponse 
            { 
                Success = false,
                Message = $"Exception during login: {ex.Message}\nStack: {ex.StackTrace}"
            };
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/auth/register", request);

            if (response.IsSuccessStatusCode)
            {
                return new AuthResponse { Success = true };
            }
            else
            {
                var errorMessage = await ParseProblemDetails(response);
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = errorMessage 
                };
            }
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

    public async Task LogoutAsync()
    {
        await tokenStorage.RemoveTokenAsync();
    }

    public async Task<string?> GetTokenAsync()
    {
        return await tokenStorage.GetTokenAsync();
    }

    private static async Task<string> ParseProblemDetails(HttpResponseMessage response)
    {
        try
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            if (problemDetails != null)
            {
                return problemDetails.Detail ?? problemDetails.Title ?? $"Error: {response.StatusCode}";
            }
        }
        catch
        {
            // If parsing fails, return status code
        }

        return $"Error: {response.StatusCode}";
    }
}