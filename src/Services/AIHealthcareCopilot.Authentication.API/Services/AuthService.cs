using Microsoft.EntityFrameworkCore;
using AIHealthcareCopilot.Authentication.API.Models;
using AIHealthcareCopilot.Shared.Models;
using AIHealthcareCopilot.Authentication.API.Data;
using BCrypt.Net;

namespace AIHealthcareCopilot.Authentication.API.Services;

public class AuthService : IAuthService
{
    private readonly HealthcareDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthService(HealthcareDbContext context, IJwtService jwtService, IConfiguration configuration)
    {
        _context = context;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Email == request.Email);

        if (doctor == null || !BCrypt.Net.BCrypt.Verify(request.Password, doctor.PasswordHash))
        {
            return null;
        }

        var token = _jwtService.GenerateToken(doctor);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryMinutes"]));

        // Store refresh token (in a real app, you'd store this in a separate table)
        doctor.RefreshToken = refreshToken;
        doctor.RefreshTokenExpiry = expiresAt.AddDays(7); // Refresh token valid for 7 days
        await _context.SaveChangesAsync();

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            Doctor = new DoctorInfo
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Email = doctor.Email,
                Specialization = doctor.Specialization,
                Hospital = doctor.Hospital
            }
        };
    }

    public async Task<Doctor?> RegisterAsync(RegisterRequest request)
    {
        // Check if doctor already exists
        if (await _context.Doctors.AnyAsync(d => d.Email == request.Email))
        {
            return null; // Doctor already exists
        }

        var doctor = new Doctor
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Specialization = request.Specialization,
            LicenseNumber = request.LicenseNumber,
            Hospital = request.Hospital,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        return doctor;
    }

    public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken)
    {
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.RefreshToken == refreshToken && 
                                d.RefreshTokenExpiry > DateTime.UtcNow);

        if (doctor == null)
        {
            return null;
        }

        var newToken = _jwtService.GenerateToken(doctor);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryMinutes"]));

        doctor.RefreshToken = newRefreshToken;
        doctor.RefreshTokenExpiry = expiresAt.AddDays(7);
        await _context.SaveChangesAsync();

        return new LoginResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            Doctor = new DoctorInfo
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Email = doctor.Email,
                Specialization = doctor.Specialization,
                Hospital = doctor.Hospital
            }
        };
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.RefreshToken == refreshToken);

        if (doctor == null)
        {
            return false;
        }

        doctor.RefreshToken = null;
        doctor.RefreshTokenExpiry = null;
        await _context.SaveChangesAsync();

        return true;
    }
}
