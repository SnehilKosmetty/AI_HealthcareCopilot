using AIHealthcareCopilot.Shared.Models;

namespace AIHealthcareCopilot.Authentication.API.Services;

public interface IJwtService
{
    string GenerateToken(Doctor doctor);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    int? GetDoctorIdFromToken(string token);
}
