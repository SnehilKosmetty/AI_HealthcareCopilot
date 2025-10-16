using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIHealthcareCopilot.PatientRecords.API.Data;
using AIHealthcareCopilot.Shared.Models;

namespace AIHealthcareCopilot.PatientRecords.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly HealthcareDbContext _context;

    public PatientsController(HealthcareDbContext context)
    {
        _context = context;
    }

    // GET: api/patients
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Patient>>> GetPatients()
    {
        return await _context.Patients.ToListAsync();
    }

    // GET: api/patients/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Patient>> GetPatient(int id)
    {
        var patient = await _context.Patients
            .Include(p => p.MedicalRecords)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
        {
            return NotFound();
        }

        return patient;
    }

    // POST: api/patients
    [HttpPost]
    public async Task<ActionResult<Patient>> PostPatient(Patient patient)
    {
        patient.CreatedAt = DateTime.UtcNow;
        patient.UpdatedAt = DateTime.UtcNow;
        
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPatient", new { id = patient.Id }, patient);
    }

    // PUT: api/patients/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPatient(int id, Patient patient)
    {
        if (id != patient.Id)
        {
            return BadRequest();
        }

        patient.UpdatedAt = DateTime.UtcNow;
        _context.Entry(patient).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PatientExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/patients/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient == null)
        {
            return NotFound();
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/patients/search?term=john
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Patient>>> SearchPatients([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return BadRequest("Search term cannot be empty");
        }

        var patients = await _context.Patients
            .Where(p => p.FirstName.Contains(term) || 
                       p.LastName.Contains(term) || 
                       p.MedicalRecordNumber.Contains(term))
            .ToListAsync();

        return patients;
    }

    private bool PatientExists(int id)
    {
        return _context.Patients.Any(e => e.Id == id);
    }
}
