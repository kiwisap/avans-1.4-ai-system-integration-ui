using avans_1._4_system_integration_ui.Models.Dto;

namespace avans_1._4_system_integration_ui.Services;

public class TokenRefreshService(HttpClient httpClient)
{
    public async Task<AccessTokenResponseDto?> RefreshAsync(string refreshToken)
    {
        try
        {
            var request = new RefreshRequestDto
            {
                RefreshToken = refreshToken
            };

            var response = await httpClient.PostAsJsonAsync("/api/identity/refresh", request);

            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<AccessTokenResponseDto>()
                : null;
        }
        catch
        {
            return null;
        }
    }
}