using System.ComponentModel.DataAnnotations;

namespace eKvarovi.Shared.Dtos;

public class AiSuggestionRequestDto
{
    [Required(ErrorMessage = "Opis kvara je obavezan.")]
    [StringLength(2000, MinimumLength = 10,
        ErrorMessage = "Opis mora imati između 10 i 2000 znakova.")]
    public string Description { get; set; } = string.Empty;
}

public class AiSuggestionResultDto
{
    public string SuggestedTitle { get; set; } = string.Empty;

    public int? SuggestedFaultTypeId { get; set; }
    public string? SuggestedFaultTypeName { get; set; }

    public int? SuggestedPriorityId { get; set; }
    public string? SuggestedPriorityName { get; set; }

    public string Reasoning { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

public class AiSummaryResultDto
{
    public string Summary { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}