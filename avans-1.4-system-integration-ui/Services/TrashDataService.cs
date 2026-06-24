using avans_1._4_system_integration_ui.Models.Dto;
using System.Text.Json;

namespace avans_1._4_system_integration_ui.Services;

public class TrashDataService(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<TrashDetectionDto>> GetTrashDataAsync(TrashDataTimeFrameDto timeframe)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/trashdata", timeframe);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<TrashDetectionDto>>(responseBody, jsonOptions);

                return result ?? [];
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"HTTP {(int)response.StatusCode}: {error}");
        }
        catch (Exception ex)
        {
          throw new Exception("HOI!", ex);
        }
    }
}