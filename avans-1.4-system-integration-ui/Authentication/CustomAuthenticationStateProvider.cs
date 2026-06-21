using avans_1._4_system_integration_ui.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace avans_1._4_system_integration_ui.Authentication;

public class CustomAuthenticationStateProvider(TokenStorageService tokenStorage, IJSRuntime jsRuntime) : AuthenticationStateProvider
{
    private AuthenticationState? _cachedState;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (jsRuntime is not IJSInProcessRuntime)
        {
            try
            {
                var token = await tokenStorage.GetTokenAsync();

                if (string.IsNullOrWhiteSpace(token))
                {
                    _cachedState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                    return _cachedState;
                }

                if (TryParseJwt(token, out var jwtClaims))
                {
                    var identity = new ClaimsIdentity(jwtClaims, "Bearer");
                    var user = new ClaimsPrincipal(identity);
                    _cachedState = new AuthenticationState(user);
                    return _cachedState;
                }

                // Treat as opaque bearer token
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, "User"),
                    new("token", token)
                };

                var opaqueIdentity = new ClaimsIdentity(claims, "Bearer");
                var opaqueUser = new ClaimsPrincipal(opaqueIdentity);
                _cachedState = new AuthenticationState(opaqueUser);
                return _cachedState;
            }
            catch
            {
                _cachedState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                return _cachedState;
            }
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    private static bool TryParseJwt(string token, out List<Claim> claims)
    {
        claims = [];

        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
            {
                return false;
            }

            var jwtToken = handler.ReadJwtToken(token);

            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                return false;
            }

            claims = jwtToken.Claims.ToList();

            if (!claims.Any(c => c.Type == ClaimTypes.Name))
            {
                var emailClaim = claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email);
                if (emailClaim != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, emailClaim.Value));
                }
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