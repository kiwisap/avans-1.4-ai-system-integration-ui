namespace avans_1._4_system_integration_ui.Models;

public class LoginResponseDto
{
    public string? TokenType { get; set; }
    public string? AccessToken { get; set; }
    public int? ExpiresIn { get; set; }
    public string? RefreshToken { get; set; }

    // Support both naming conventions
    public string? Token { get; set; }
}