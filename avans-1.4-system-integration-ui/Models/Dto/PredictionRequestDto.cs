namespace avans_1._4_system_integration_ui.Models.Dto;

public class PredictionRequestDto
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string? TrashType { get; set; }

    public double? Temperature { get; set; }

    public string? WeatherType { get; set; }

    public DateTimeOffset DateTime { get; set; }

    public string? Model { get; set; }
}