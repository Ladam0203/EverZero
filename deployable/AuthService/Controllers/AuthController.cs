using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

using Services;

[Route("api/auth")]
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
}