namespace AuthService.Core.DTOs;

public class RegisterRequest {
    /// <summary>
    /// The user's email address.
    /// </summary>
    /// <example>john.doe@example</example>
    public string Email { get; set; } = default!;
    
    /// <summary>
    /// The user's username.
    /// </summary>
    /// <example>john.doe</example>
    public string Username { get; set; } = default!;

    /// <summary>
    /// The user's password.
    /// </summary>
    /// <example>Passw0rd!</example>
    public string Password { get; set; } = default!;    
}