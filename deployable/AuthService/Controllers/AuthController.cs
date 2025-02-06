using AuthService.Services.Interfaces;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using RegisterRequest = AuthService.Core.DTOs.RegisterRequest;
using LoginRequest = AuthService.Core.DTOs.LoginRequest;
using Messages;

namespace AuthService.Controllers;

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    
    public AuthController(IAuthService authService, IJwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
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
        var claimsPrincipal = _jwtService.ValidateTokenAsync(token);
        if (claimsPrincipal == null)
            return Unauthorized("Invalid token.");
        
        var claimDtos = claimsPrincipal.Claims.Select(c => new ClaimDto(c.Type, c.Value));
        
        return Ok(new AuthorizationResponse(claimDtos));
    }

}