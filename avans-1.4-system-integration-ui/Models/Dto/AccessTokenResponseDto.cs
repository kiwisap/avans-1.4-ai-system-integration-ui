namespace avans_1._4_system_integration_ui.Models.Dto;

public class AccessTokenResponseDto
{
    public string TokenType { get; set; } = "Bearer";

    public string AccessToken { get; set; } = string.Empty;

    public long ExpiresIn { get; set; }

    public string RefreshToken { get; set; } = string.Empty;
}