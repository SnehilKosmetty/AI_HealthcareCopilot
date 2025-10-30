using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AIHealthcareCopilot.Shared.Contracts;
using System.Net.Http.Json;

namespace AIHealthcareCopilot.AIAnalysis.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly IAnalysisService _analysisService;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AnalysisController(IAnalysisService analysisService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _analysisService = analysisService;
        _httpClient = httpClientFactory.CreateClient("PatientsApi");
        _configuration = configuration;
    }

    public record SummarizeRequest(string PatientNotes);
    public record DetectMissingRequest(string PatientNotes);
    public record RecommendRequest(string PatientNotes, string Assessment);
    public record RunAndSaveRequest(int MedicalRecordId, string PatientNotes, string? Assessment);

    [HttpPost("summarize")]
    public async Task<ActionResult<object>> Summarize([FromBody] SummarizeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PatientNotes)) return BadRequest("PatientNotes is required.");
        var summary = await _analysisService.GenerateSummaryAsync(request.PatientNotes);
        return Ok(new { summary });
    }

    [HttpPost("detect-missing")]
    public async Task<ActionResult<object>> DetectMissing([FromBody] DetectMissingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PatientNotes)) return BadRequest("PatientNotes is required.");
        var missing = await _analysisService.DetectMissingDetailsAsync(request.PatientNotes);
        return Ok(new { missing });
    }

    [HttpPost("recommend")]
    public async Task<ActionResult<object>> Recommend([FromBody] RecommendRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PatientNotes)) return BadRequest("PatientNotes is required.");
        var suggestions = await _analysisService.GenerateSuggestionsAsync(request.PatientNotes, request.Assessment ?? string.Empty);
        return Ok(new { suggestions });
    }

    [HttpPost("run-and-save")]
    public async Task<ActionResult<object>> RunAndSave([FromBody] RunAndSaveRequest request)
    {
        if (request.MedicalRecordId <= 0) return BadRequest("MedicalRecordId is required.");
        if (string.IsNullOrWhiteSpace(request.PatientNotes)) return BadRequest("PatientNotes is required.");

        var summary = await _analysisService.GenerateSummaryAsync(request.PatientNotes);
        var missing = await _analysisService.DetectMissingDetailsAsync(request.PatientNotes);
        var suggestions = await _analysisService.GenerateSuggestionsAsync(request.PatientNotes, request.Assessment ?? string.Empty);

        // Persist three AnalysisResult records via PatientRecords API (through Gateway base URL)
        async Task<int> PostResult(string type, string result, string confidence)
        {
            var payload = new { medicalRecordId = request.MedicalRecordId, analysisType = type, result, confidence };
            var resp = await _httpClient.PostAsJsonAsync("/api/analysisresults", payload);
            resp.EnsureSuccessStatusCode();
            var created = await resp.Content.ReadFromJsonAsync<CreateResponse>();
            return created?.Id ?? 0;
        }

        var savedIds = new List<int>();
        savedIds.Add(await PostResult("Summary", summary, "N/A"));
        savedIds.Add(await PostResult("MissingDetails", string.Join(", ", missing), "N/A"));
        savedIds.Add(await PostResult("Suggestions", string.Join("; ", suggestions), "N/A"));

        return Ok(new { saved = savedIds, summary, missing, suggestions });
    }

    private record CreateResponse(int Id);
}


