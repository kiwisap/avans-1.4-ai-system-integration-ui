using avans_1._4_system_integration_ui.Models.Dto;

namespace avans_1._4_system_integration_ui.Services;

public class TrashDataService(
    HttpClient httpClient,
    TokenStorageService tokenStorage,
    TokenRefreshService refresher)
    : AbstractAuthenticatedApiService(httpClient, tokenStorage, refresher)
{
    public async Task<List<TrashDetectionDto>> GetTrashDataAsync(TrashDataTimeFrameDto timeframe)
        => await PostAsync<List<TrashDetectionDto>>("api/trashdata", timeframe) ?? [];
}