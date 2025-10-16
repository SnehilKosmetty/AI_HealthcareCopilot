namespace AIHealthcareCopilot.Shared.Models;

public class MedicalRecord
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateTime VisitDate { get; set; }
    public string ChiefComplaint { get; set; } = string.Empty;
    public string HistoryOfPresentIllness { get; set; } = string.Empty;
    public string PhysicalExamination { get; set; } = string.Empty;
    public string Assessment { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public ICollection<AnalysisResult> AnalysisResults { get; set; } = new List<AnalysisResult>();
}
