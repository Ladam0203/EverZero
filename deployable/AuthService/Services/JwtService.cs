using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Identity;
using AuthService.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    
    public JwtService(IConfiguration config)
    {
        _config = config;
    }
    
    public string GenerateToken(User user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
        };
        
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:ExpirationMinutes"));

        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: expires,
            signingCredentials: signingCredentials
        );

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