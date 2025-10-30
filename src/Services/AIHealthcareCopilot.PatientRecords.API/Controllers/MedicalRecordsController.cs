using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AIHealthcareCopilot.PatientRecords.API.Data;
using AIHealthcareCopilot.Shared.Models;

namespace AIHealthcareCopilot.PatientRecords.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MedicalRecordsController : ControllerBase
{
    private readonly HealthcareDbContext _context;

    public MedicalRecordsController(HealthcareDbContext context)
    {
        _context = context;
    }

    // DTOs
    public record CreateMedicalRecordDto(
        int PatientId,
        int DoctorId,
        DateTime VisitDate,
        string? ChiefComplaint,
        string? HistoryOfPresentIllness,
        string? PhysicalExamination,
        string? Assessment,
        string? Plan,
        string? Notes
    );

    public record UpdateMedicalRecordDto(
        DateTime VisitDate,
        string? ChiefComplaint,
        string? HistoryOfPresentIllness,
        string? PhysicalExamination,
        string? Assessment,
        string? Plan,
        string? Notes
    );

    // GET: api/medicalrecords
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MedicalRecord>>> GetAll()
    {
        var list = await _context.MedalRecordsQuery().ToListAsync();
        return Ok(list);
    }

    // GET: api/medicalrecords/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MedicalRecord>> GetById(int id)
    {
        var mr = await _context.MedalRecordsQuery().FirstOrDefaultAsync(x => x.Id == id);
        if (mr == null) return NotFound();
        return Ok(mr);
    }

    // POST: api/medicalrecords
    [HttpPost]
    public async Task<ActionResult<MedicalRecord>> Create([FromBody] CreateMedicalRecordDto dto)
    {
        var patientExists = await _context.Patients.AnyAsync(p => p.Id == dto.PatientId);
        if (!patientExists) return NotFound($"Patient {dto.PatientId} not found");
        var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == dto.DoctorId);
        if (!doctorExists) return NotFound($"Doctor {dto.DoctorId} not found");

        var entity = new MedicalRecord
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            VisitDate = dto.VisitDate,
            ChiefComplaint = dto.ChiefComplaint ?? string.Empty,
            HistoryOfPresentIllness = dto.HistoryOfPresentIllness ?? string.Empty,
            PhysicalExamination = dto.PhysicalExamination ?? string.Empty,
            Assessment = dto.Assessment ?? string.Empty,
            Plan = dto.Plan ?? string.Empty,
            Notes = dto.Notes ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MedicalRecords.Add(entity);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    // PUT: api/medicalrecords/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicalRecordDto dto)
    {
        var entity = await _context.MedicalRecords.FindAsync(id);
        if (entity == null) return NotFound();

        entity.VisitDate = dto.VisitDate;
        entity.ChiefComplaint = dto.ChiefComplaint ?? string.Empty;
        entity.HistoryOfPresentIllness = dto.HistoryOfPresentIllness ?? string.Empty;
        entity.PhysicalExamination = dto.PhysicalExamination ?? string.Empty;
        entity.Assessment = dto.Assessment ?? string.Empty;
        entity.Plan = dto.Plan ?? string.Empty;
        entity.Notes = dto.Notes ?? string.Empty;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/medicalrecords/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.MedicalRecords.FindAsync(id);
        if (entity == null) return NotFound();

        _context.MedicalRecords.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

internal static class MedicalRecordQueries
{
    public static IQueryable<MedicalRecord> MedalRecordsQuery(this HealthcareDbContext ctx)
    {
        return ctx.MedicalRecords.AsNoTracking();
    }
}


