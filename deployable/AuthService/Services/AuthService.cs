using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Core;
using AuthService.Core.DTOs;
using AuthService.Identity;
using AuthService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RegisterRequest = AuthService.Core.DTOs.RegisterRequest;

namespace AuthService.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtService _jwtService;

    public AuthService(UserManager<User> userManager, 
        SignInManager<User> signInManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthResponse>> RegisterUserAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) 
            || string.IsNullOrWhiteSpace(request.Username)
            || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure(new[] { "Email, username, and password are required." });
        }
        // Identity validates the rest of the fields in-depth (password complexity, email format, etc.)
        
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
        
        // Add to role
        _userManager.AddToRoleAsync(user, "User").Wait();
        
        var roles = await _userManager.GetRolesAsync(user);

        return Result<AuthResponse>.Success(new AuthResponse(
            user.Id,
            user.Email,
            user.UserName,
            roles,
            _jwtService.GenerateToken(user, roles)
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
        
        var roles = await _userManager.GetRolesAsync(user);
        
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
            roles,
            _jwtService.GenerateToken(user, roles)
        ));
    }
}