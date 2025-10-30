using Azure.AI.TextAnalytics;
using AIHealthcareCopilot.Shared.Contracts;

namespace AIHealthcareCopilot.AIAnalysis.API.Services;

public class AzureTextAnalyticsAnalysisService : IAnalysisService
{
    private readonly TextAnalyticsClient? _client;

    public AzureTextAnalyticsAnalysisService(TextAnalyticsClient? client)
    {
        _client = client;
    }

    public async Task<string> GenerateSummaryAsync(string patientNotes)
    {
        if (_client is null || string.IsNullOrWhiteSpace(patientNotes))
        {
            return FallbackSummarize(patientNotes);
        }

        try
        {
            // Simple extractive summarization fallback using key phrases as bullets
            var response = await _client.ExtractKeyPhrasesAsync(patientNotes);
            var bullets = string.Join("; ", response.Value.Take(10));
            return string.IsNullOrWhiteSpace(bullets) ? FallbackSummarize(patientNotes) : $"Key points: {bullets}";
        }
        catch
        {
            return FallbackSummarize(patientNotes);
        }
    }

    public async Task<List<string>> DetectMissingDetailsAsync(string patientNotes)
    {
        var missing = new List<string>();
        var text = (patientNotes ?? string.Empty).ToLowerInvariant();

        static bool ContainsAny(string text, params string[] keys) => keys.Any(k => text.Contains(k));

        bool hasChiefComplaint = ContainsAny(text, "chief complaint", "cc:");
        if (!hasChiefComplaint) missing.Add("Chief complaint");

        bool hasHpi = ContainsAny(text, "hpi", "history of present illness");
        if (!hasHpi) missing.Add("History of present illness");

        // Past medical history: explicit markers OR common chronic condition mention
        string[] conditionHints = new[] { "diabetes", "hypertension", "asthma", "copd", "cad", "ckd", "stroke", "mi ", "thyroid", "cancer" };
        bool hasPmh = ContainsAny(text, "pmh", "past medical history") || ContainsAny(text, conditionHints);
        if (!hasPmh) missing.Add("Past medical history");

        // Medications: explicit markers OR pattern "on <drug>" OR drug keywords
        string[] medHints = new[] { "meds", "medications", "rx:", "prescribed" };
        bool mentionsOnDrug = System.Text.RegularExpressions.Regex.IsMatch(text, @"\bon\s+[a-z][a-z0-9\-]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        string[] commonDrugs = new[] { "metformin", "lisinopril", "atorvastatin", "amlodipine", "insulin", "metoprolol" };
        bool hasMeds = ContainsAny(text, medHints) || mentionsOnDrug || ContainsAny(text, commonDrugs);
        if (!hasMeds) missing.Add("Medications");

        // Allergies
        bool hasAllergies = ContainsAny(text, "allergies", "nka", "nkd a", "nkda");
        if (!hasAllergies) missing.Add("Allergies");

        // Vitals: explicit vitals keywords. If text says "no vitals" keep it missing
        bool hasVitals = ContainsAny(text, "bp", "blood pressure", "pulse", "hr ", "heart rate", "temperature", "temp ", "fever", "spo2", "o2", "resp", "rr ");
        if (!hasVitals) missing.Add("Vitals");

        bool hasPE = ContainsAny(text, "pe:", "physical exam", "exam:");
        if (!hasPE) missing.Add("Physical exam");

        bool hasAssessment = ContainsAny(text, "assessment", "impression");
        if (!hasAssessment) missing.Add("Assessment/Impression");

        bool hasPlan = ContainsAny(text, "plan:", "plan ");
        if (!hasPlan) missing.Add("Plan");

        return missing;
    }

    public async Task<List<string>> GenerateSuggestionsAsync(string patientNotes, string assessment)
    {
        var suggestions = new List<string>();
        var text = (patientNotes + " " + assessment).ToLowerInvariant();

        if (text.Contains("hypertension") || text.Contains("high blood pressure"))
        {
            suggestions.Add("Check recent BP trend; ensure lifestyle counseling and medication adherence.");
            suggestions.Add("Order BMP, lipid panel if not recent; review secondary causes if resistant.");
        }
        if (text.Contains("diabetes") || text.Contains("hyperglycemia"))
        {
            suggestions.Add("Review A1c and kidney function; foot and eye exam as indicated.");
            suggestions.Add("Discuss hypoglycemia risk and individualized glycemic targets.");
        }
        if (text.Contains("chest pain"))
        {
            suggestions.Add("Assess ACS risk; obtain ECG; consider troponin and risk stratification.");
        }

        if (_client != null)
        {
            // Light enrichment: add top key phrases as areas to explore
            try
            {
                var resp = await _client.ExtractKeyPhrasesAsync(patientNotes);
                foreach (var phrase in resp.Value.Take(5))
                {
                    suggestions.Add($"Explore: {phrase}");
                }
            }
            catch { /* ignore enrichment failures */ }
        }

        if (suggestions.Count == 0)
        {
            suggestions.Add("No specific suggestions generated. Consider clarifying HPI, vitals, and red flags.");
        }

        return suggestions;
    }

    private static string FallbackSummarize(string input)
    {
        var text = (input ?? string.Empty).Trim();
        if (text.Length <= 200) return text;
        return text.Substring(0, 200) + "...";
    }
}


