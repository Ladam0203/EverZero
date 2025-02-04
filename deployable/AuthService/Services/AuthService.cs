using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Core;
using AuthService.Core.DTOs;
using AuthService.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RegisterRequest = Microsoft.AspNetCore.Identity.Data.RegisterRequest;

namespace AuthService.Services;

public class AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _config;

    public AuthService(UserManager<User> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    public async Task<Result<AuthResponse>> RegisterUserAsync(RegisterRequest request)
    {
        var user = new User
        {
            Email = request.Email,
            UserName = request.Email,
            EmailConfirmed = true, // for simplicity, we're confirming the email automatically
            IsActive = true // user is active by default
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            return Result<AuthResponse>.Failure(result.Errors.Select(e => e.Description));
        }
        
        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id,
            user.Email,
            user.UserName,
            GenerateToken(user)
        ));
    }

    /*
    public async Task<string?> LoginUserAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            return null;

        return GenerateToken(user);
    }
    */

    private string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}