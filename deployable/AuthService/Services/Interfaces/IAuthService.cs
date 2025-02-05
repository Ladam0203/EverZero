using AuthService.Core;
using AuthService.Core.DTOs;

namespace AuthService.Services.Interfaces;

public interface IAuthService
{
    public Task<Result<AuthResponse>> RegisterUserAsync(RegisterRequest request);
    public Task<Result<AuthResponse>> LoginUserAsync(LoginRequest request);
}