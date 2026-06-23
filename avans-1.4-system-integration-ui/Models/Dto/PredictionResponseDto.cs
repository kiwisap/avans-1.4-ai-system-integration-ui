namespace avans_1._4_system_integration_ui.Models.Dto;

public class PredictionResponseDto
{
    public string? BasePriority { get; set; }

    public string? FinalPriority { get; set; }

    public string? Model { get; set; }

    public LocationInfoDto? Location { get; set; }

    public string? KnownEvent { get; set; }

    public List<EventInfoDto> NearbyEvents { get; set; } = new();

    public string? Explanation { get; set; }

    public List<ProbabilityItemDto> PriorityProbabilities { get; set; } = new();

    public string? TrashType { get; set; }

    public bool TrashTypeProvided { get; set; }

    public List<ProbabilityItemDto> TrashTypeProbabilities { get; set; } = new();
}