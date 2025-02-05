using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using RegisterRequest = AuthService.Core.DTOs.RegisterRequest;
using LoginRequest = AuthService.Core.DTOs.LoginRequest;

namespace AuthService.Controllers;

using Services;

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterUserAsync(request);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Value);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginUserAsync(request);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Errors);
        }
        return Ok(result.Value);
    }
    
    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Unauthorized("Missing Authorization header.");

        var authHeader = Request.Headers["Authorization"].ToString();
        if (!authHeader.StartsWith("Bearer "))
            return Unauthorized("Invalid Authorization header.");

        var token = authHeader.Replace("Bearer ", "");

        // Validate token and extract claims
        var userClaims = _authService.ValidateTokenAsync(token);
        if (userClaims == null)
            return Unauthorized("Invalid token.");

        return Ok(new
        {
            IsAuthenticated = true,
            Claims = userClaims.Claims.Select(c => new { c.Type, c.Value })
        });
    }

}