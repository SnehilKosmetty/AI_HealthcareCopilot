using Microsoft.AspNetCore.Mvc;
using AIHealthcareCopilot.Authentication.API.Models;
using AIHealthcareCopilot.Authentication.API.Services;

namespace AIHealthcareCopilot.Authentication.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest request)
    {
        try
        {
            var doctor = await _authService.RegisterAsync(request);
            
            if (doctor == null)
            {
                return BadRequest(new { message = "Doctor with this email already exists" });
            }

            return CreatedAtAction(nameof(Login), new { id = doctor.Id }, new { 
                message = "Doctor registered successfully",
                doctorId = doctor.Id 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> RefreshToken(RefreshTokenRequest request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "An error occurred during token refresh" });
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout(RefreshTokenRequest request)
    {
        try
        {
            var success = await _authService.LogoutAsync(request.RefreshToken);
            
            if (!success)
            {
                return BadRequest(new { message = "Invalid refresh token" });
            }

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    [HttpGet("validate")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public ActionResult ValidateToken()
    {
        // This endpoint validates if a token is still valid
        // The [Authorize] attribute ensures JWT middleware validates the token
        var doctorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var specialization = User.FindFirst("Specialization")?.Value;
        var hospital = User.FindFirst("Hospital")?.Value;

        return Ok(new { 
            message = "Token is valid",
            doctorId = doctorId,
            email = email,
            name = name,
            specialization = specialization,
            hospital = hospital,
            validatedAt = DateTime.UtcNow
        });
    }
}
