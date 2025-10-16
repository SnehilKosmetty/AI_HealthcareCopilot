using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIHealthcareCopilot.PatientRecords.API.Data;
using AIHealthcareCopilot.Shared.Models;
using AIHealthcareCopilot.PatientRecords.API.DTOs;

namespace AIHealthcareCopilot.PatientRecords.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly HealthcareDbContext _context;

    public DoctorsController(HealthcareDbContext context)
    {
        _context = context;
    }

    // GET: api/doctors
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetDoctors()
    {
        var doctors = await _context.Doctors
            .Select(d => new DoctorResponseDto
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                Email = d.Email,
                Specialization = d.Specialization,
                LicenseNumber = d.LicenseNumber,
                Hospital = d.Hospital,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync();
        
        return Ok(doctors);
    }

    // GET: api/doctors/5
    [HttpGet("{id}")]
    public async Task<ActionResult<DoctorResponseDto>> GetDoctor(int id)
    {
        var doctor = await _context.Doctors
            .Where(d => d.Id == id)
            .Select(d => new DoctorResponseDto
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                Email = d.Email,
                Specialization = d.Specialization,
                LicenseNumber = d.LicenseNumber,
                Hospital = d.Hospital,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (doctor == null)
        {
            return NotFound();
        }

        return Ok(doctor);
    }

    // POST: api/doctors
    [HttpPost]
    public async Task<ActionResult<DoctorResponseDto>> PostDoctor(DoctorDto doctorDto)
    {
        // Create a new doctor without authentication fields
        var newDoctor = new Doctor
        {
            FirstName = doctorDto.FirstName,
            LastName = doctorDto.LastName,
            Email = doctorDto.Email,
            Specialization = doctorDto.Specialization,
            LicenseNumber = doctorDto.LicenseNumber,
            Hospital = doctorDto.Hospital,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Doctors.Add(newDoctor);
        await _context.SaveChangesAsync();

        var result = new DoctorResponseDto
        {
            Id = newDoctor.Id,
            FirstName = newDoctor.FirstName,
            LastName = newDoctor.LastName,
            Email = newDoctor.Email,
            Specialization = newDoctor.Specialization,
            LicenseNumber = newDoctor.LicenseNumber,
            Hospital = newDoctor.Hospital,
            CreatedAt = newDoctor.CreatedAt,
            UpdatedAt = newDoctor.UpdatedAt
        };

        return CreatedAtAction("GetDoctor", new { id = newDoctor.Id }, result);
    }

    // PUT: api/doctors/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDoctor(int id, DoctorDto doctorDto)
    {
        var existingDoctor = await _context.Doctors.FindAsync(id);
        if (existingDoctor == null)
        {
            return NotFound();
        }

        // Update only non-authentication fields
        existingDoctor.FirstName = doctorDto.FirstName;
        existingDoctor.LastName = doctorDto.LastName;
        existingDoctor.Email = doctorDto.Email;
        existingDoctor.Specialization = doctorDto.Specialization;
        existingDoctor.LicenseNumber = doctorDto.LicenseNumber;
        existingDoctor.Hospital = doctorDto.Hospital;
        existingDoctor.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DoctorExists(id))
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

    // DELETE: api/doctors/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        var doctor = await _context.Doctors.FindAsync(id);
        if (doctor == null)
        {
            return NotFound();
        }

        _context.Doctors.Remove(doctor);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool DoctorExists(int id)
    {
        return _context.Doctors.Any(e => e.Id == id);
    }
}