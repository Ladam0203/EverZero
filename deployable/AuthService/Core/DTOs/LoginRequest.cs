namespace AuthService.Core.DTOs;

public class LoginRequest
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    /// <example>john.doe@example</example>
    public string Email { get; set; } = default!;

    /// <summary>
    /// The user's password.
    /// </summary>
    /// <example>Passw0rd!</example>
    public string Password { get; set; } = default!;    
}