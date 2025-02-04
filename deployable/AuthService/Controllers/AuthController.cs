using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

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

    /*
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthRequest request)
    {
        var token = await _authService.LoginUserAsync(request.Email, request.Password);
        if (token == null) return Unauthorized("Invalid credentials.");

        return Ok(new { Token = token });
    }
    */
    
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