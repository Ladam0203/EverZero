using System.Security.Claims;
using AuthService.Identity;

namespace AuthService.Services.Interfaces;

public interface IJwtService
{
    public string GenerateToken(User user, IList<string> roles);
    public ClaimsPrincipal? ValidateTokenAsync(string token);
}