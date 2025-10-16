namespace AIHealthcareCopilot.Shared.Models;

public class AnalysisResult
{
    public int Id { get; set; }
    public int MedicalRecordId { get; set; }
    public string AnalysisType { get; set; } = string.Empty; // Summary, MissingDetails, Suggestions
    public string Result { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public MedicalRecord MedicalRecord { get; set; } = null!;
}
