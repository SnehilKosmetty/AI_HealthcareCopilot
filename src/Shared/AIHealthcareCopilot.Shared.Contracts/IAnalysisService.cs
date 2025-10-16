using AIHealthcareCopilot.Shared.Models;

namespace AIHealthcareCopilot.Shared.Contracts;

public interface IAnalysisService
{
    Task<string> GenerateSummaryAsync(string patientNotes);
    Task<List<string>> DetectMissingDetailsAsync(string patientNotes);
    Task<List<string>> GenerateSuggestionsAsync(string patientNotes, string assessment);
}
