using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace avans_1._4_system_integration_ui.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenStorageService _tokenStorage;

        public AuthService(HttpClient httpClient, TokenStorageService tokenStorage)
        {
            _httpClient = httpClient;
            _tokenStorage = tokenStorage;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/identity/login", request);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<LoginResponse>(responseBody, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });

                    var token = result?.AccessToken ?? result?.Token;

                    if (!string.IsNullOrEmpty(token))
                    {
                        await _tokenStorage.SetTokenAsync(token);
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

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/register", request);

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
            await _tokenStorage.RemoveTokenAsync();
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _tokenStorage.GetTokenAsync();
        }

        private async Task<string> ParseProblemDetails(HttpResponseMessage response)
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

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string? TokenType { get; set; }
        public string? AccessToken { get; set; }
        public int? ExpiresIn { get; set; }
        public string? RefreshToken { get; set; }

        // Support both naming conventions
        public string? Token { get; set; }
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
