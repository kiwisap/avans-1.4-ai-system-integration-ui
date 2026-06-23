using avans_1._4_system_integration_ui.Models.Dto;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace avans_1._4_system_integration_ui.Services;

public class PredictionsService(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    public async Task<PredictionResponseDto> PredictAsync(PredictionRequestDto request)
    {
        using var response = await httpClient.PostAsJsonAsync("predict", request, JsonOptions);

        //if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        //{
        //    var error = await response.Content
        //        .ReadFromJsonAsync<ValidationErrorResponse>(JsonOptions, cancellationToken);
        //    throw new PredictionValidationException(error);
        //}

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<PredictionResponseDto>(JsonOptions);

        return result
            ?? throw new InvalidOperationException("Lege response van de prediction-API.");
    }
}