using avans_1._4_system_integration_ui.Services;
using System.Net.Http.Headers;

namespace avans_1._4_system_integration_ui.Http;

public class AuthorizationMessageHandler(TokenStorageService tokenStorage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
     HttpRequestMessage request,
     CancellationToken cancellationToken)
    {
        try
        {
            var token = await tokenStorage.GetTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // JS interop nog niet beschikbaar, request zonder token sturen
        }

        return await base.SendAsync(request, cancellationToken);
    }
}