using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Core;
using AuthService.Core.DTOs;
using AuthService.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RegisterRequest = AuthService.Core.DTOs.RegisterRequest;

namespace AuthService.Services;

public class AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _config;

    public AuthService(UserManager<User> userManager, 
        SignInManager<User> signInManager,
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
    }

    public async Task<Result<AuthResponse>> RegisterUserAsync(RegisterRequest request)
    {
        var user = new User
        {
            Email = request.Email,
            UserName = request.Username,
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

    public async Task<Result<AuthResponse>> LoginUserAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure(new[] { "Invalid email or password." });
        }
        
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result<AuthResponse>.Failure(new[] { "Invalid email or password." });
        }
        if (!user.IsActive)
        {
            return Result<AuthResponse>.Failure(new[] { "User is inactive." });
        }
        
        // No roles
        
        var error = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        
        // No 2FA
        
        if (!error.Succeeded)
        {
            return Result<AuthResponse>.Failure(new[] { "Invalid email or password." });
        }
        
        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id,
            user.Email,
            user.UserName,
            GenerateToken(user)
        ));
    }

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
            expires: DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:ExpirationInMinutes")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public ClaimsPrincipal? ValidateTokenAsync(string token) {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]));
        var validationParameters = new TokenValidationParameters {
            ValidateIssuer = true,  // Ensure issuer is validated
            ValidateAudience = true,  // Ensure audience is validated
            ValidateLifetime = true,  // Ensure token expiration is checked
            ValidateIssuerSigningKey = true,  // Ensure the signing key is validated
            ValidIssuer = _config["Jwt:Issuer"],  // Valid issuer from settings
            ValidAudience = _config["Jwt:Audience"],  // Valid audience from settings
            IssuerSigningKey = key,  // The key used to sign the token
            ClockSkew = TimeSpan.Zero  // No clock skew (for precise expiration time matching)
        };

        try {
            // Validate the token and return the ClaimsPrincipal if valid
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        } catch {
            // Return null if token validation fails
            return null;
        }
    }
}