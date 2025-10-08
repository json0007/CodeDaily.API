using CodeDaily.API.Domain.Services;
using CodeDaily.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeDaily.API.Features.Auth;

[Route("api/admin/[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
 
        var authResult = await authService.AuthenticateAsync(request.Email, request.Password);
 
         if (!authResult.IsAuthenticated)
         {
             return Unauthorized(new { message = authResult.ErrorMessage });
         }
 
         return Ok(new AuthResponse
         {
             Token = authResult.Token,
             ExpiresAt = authResult.ExpiresAt
         });
    }
    
    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var registerResult = await authService.RegisterUserAsync(request.Email, request.Password);

        if (!registerResult.IsSuccessful)
        {
            return Conflict(new { message = registerResult.ErrorMessage });
        }
        
        return Ok(registerResult.UserId);
    }
}