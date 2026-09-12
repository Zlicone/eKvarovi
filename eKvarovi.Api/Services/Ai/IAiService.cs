using eKvarovi.Shared.Dtos;

namespace eKvarovi.Api.Services.Ai;

public interface IAiService
{
    string ProviderName { get; }

    Task<AiSuggestionResultDto> SuggestReportAsync(
        string description,
        CancellationToken cancellationToken = default);

    Task<AiSummaryResultDto> SummarizeAssignmentAsync(
        int assignmentId,
        CancellationToken cancellationToken = default);
}