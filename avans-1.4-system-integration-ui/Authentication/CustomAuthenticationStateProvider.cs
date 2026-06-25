using avans_1._4_system_integration_ui.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace avans_1._4_system_integration_ui.Authentication;

public class CustomAuthenticationStateProvider(
    TokenStorageService tokenStorage,
    AuthService authService,
    IJSRuntime jsRuntime) : AuthenticationStateProvider
{
    private AuthenticationState? _cachedState;

    private static AuthenticationState Anonymous =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // In-process runtime (WASM prerender path) → can't read localStorage; treat as anonymous.
        if (jsRuntime is IJSInProcessRuntime)
            return Anonymous;

        try
        {
            var tokens = await tokenStorage.GetTokensAsync();
            var token = tokens?.AccessToken;

            if (string.IsNullOrWhiteSpace(token))
            {
                _cachedState = Anonymous;
                return _cachedState;
            }

            // --- JWT path: claims (incl. name/email) come straight from the token ---
            if (TryParseJwt(token, out var jwtClaims))
            {
                var identity = new ClaimsIdentity(jwtClaims, "Bearer");
                _cachedState = new AuthenticationState(new ClaimsPrincipal(identity));
                return _cachedState;
            }

            var user = await authService.GetCurrentUserAsync();

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user?.Name ?? user?.Email ?? "Gebruiker"),
                new("token", token)
            };

            var opaqueIdentity = new ClaimsIdentity(claims, "Bearer");
            _cachedState = new AuthenticationState(new ClaimsPrincipal(opaqueIdentity));
            return _cachedState;
        }
        catch
        {
            // Interop unavailable (prerender) or any failure → anonymous, retried on next interactive render.
            _cachedState = Anonymous;
            return _cachedState;
        }
    }

    private static bool TryParseJwt(string token, out List<Claim> claims)
    {
        claims = [];

        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
                return false;

            var jwtToken = handler.ReadJwtToken(token);

            if (jwtToken.ValidTo < DateTime.UtcNow)
                return false;

            claims = jwtToken.Claims.ToList();

            // Ensure there's a Name claim so @context.User.Identity?.Name works.
            if (!claims.Any(c => c.Type == ClaimTypes.Name))
            {
                var emailClaim = claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email);
                if (emailClaim != null)
                    claims.Add(new Claim(ClaimTypes.Name, emailClaim.Value));
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void NotifyAuthenticationStateChanged()
    {
        _cachedState = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}