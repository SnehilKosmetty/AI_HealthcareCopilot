using AIHealthcareCopilot.Authentication.API.Models;
using AIHealthcareCopilot.Shared.Models;

namespace AIHealthcareCopilot.Authentication.API.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<Doctor?> RegisterAsync(RegisterRequest request);
    Task<LoginResponse?> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(string refreshToken);
}
