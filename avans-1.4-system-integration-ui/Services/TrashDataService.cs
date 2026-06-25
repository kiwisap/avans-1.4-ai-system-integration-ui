using avans_1._4_system_integration_ui.Authentication;
using avans_1._4_system_integration_ui.Models.Dto;
using System.Net.Http.Headers;
using System.Text.Json;

namespace avans_1._4_system_integration_ui.Services;

public class TrashDataService(HttpClient httpClient, CustomAuthenticationStateProvider authStateProvider)
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<TrashDetectionDto>> GetTrashDataAsync(TrashDataTimeFrameDto timeframe)
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var authToken = authState.User.Claims.FirstOrDefault(c => c.Type == "token")?.Value;

        var request = new HttpRequestMessage(HttpMethod.Post, "api/trashdata")
        {
            Content = JsonContent.Create(timeframe, options: jsonOptions)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TrashDetectionDto>>(body, jsonOptions) ?? [];
        }

        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"HTTP {(int)response.StatusCode}: {error}");
    }
}