namespace avans_1._4_system_integration_ui.Models;

public record AuthTokens(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt);