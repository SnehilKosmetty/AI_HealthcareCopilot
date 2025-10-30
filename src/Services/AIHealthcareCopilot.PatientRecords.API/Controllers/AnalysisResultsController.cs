using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AIHealthcareCopilot.PatientRecords.API.Data;
using AIHealthcareCopilot.Shared.Models;

namespace AIHealthcareCopilot.PatientRecords.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AnalysisResultsController : ControllerBase
{
    private readonly HealthcareDbContext _context;

    public AnalysisResultsController(HealthcareDbContext context)
    {
        _context = context;
    }

    public record CreateAnalysisResultDto(int MedicalRecordId, string AnalysisType, string Result, string Confidence);

    [HttpPost]
    public async Task<ActionResult<AnalysisResult>> Create([FromBody] CreateAnalysisResultDto dto)
    {
        var exists = await _context.MedicalRecords.AnyAsync(m => m.Id == dto.MedicalRecordId);
        if (!exists) return NotFound($"MedicalRecord {dto.MedicalRecordId} not found");

        var entity = new AnalysisResult
        {
            MedicalRecordId = dto.MedicalRecordId,
            AnalysisType = dto.AnalysisType,
            Result = dto.Result,
            Confidence = dto.Confidence,
            CreatedAt = DateTime.UtcNow
        };

        _context.AnalysisResults.Add(entity);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AnalysisResult>> GetById(int id)
    {
        var ar = await _context.AnalysisResults.FindAsync(id);
        if (ar == null) return NotFound();
        return Ok(ar);
    }
}


