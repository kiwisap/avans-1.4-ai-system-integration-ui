using System.Net.Http.Headers;

namespace avans_1._4_system_integration_ui.Services
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly TokenStorageService _tokenStorage;

        public AuthorizationMessageHandler(TokenStorageService tokenStorage)
        {
            _tokenStorage = tokenStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _tokenStorage.GetTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
