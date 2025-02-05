namespace AuthService.Core.DTOs;

public class AuthResponse
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The email of the user.
    /// </summary>
    /// <example>user@example.dk</example>
    public string Email { get; set; }
    
    /// <summary>
    /// The user's username.
    /// </summary>
    /// <example>john.doe</example>
    public string Username { get; set; } = default!;
    
    /// <summary>
    /// The roles of the user (User, Admin).
    /// </summary>
    /// <example>["User"]</example>
    public IList<string>? Roles { get; set; }

    /// <summary>
    /// The token of the user.
    /// </summary>
    public string TokenType => "Bearer";
    
    /// <summary>
    /// The token of the user.
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Initializes a new instance of the LoginSuccessDto class.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="username">The username of the user.</param>
    /// <param name="email">The email of the user.</param>
    /// <param name="roles">The roles of the user.</param>
    /// <param name="token">The token of the user.</param>
    public AuthResponse(Guid id, string email, string username, IList<string> roles, string token) {
        Id = id;
        Email = email;
        Username = username;
        Roles = roles;
        Token = token;
    }
}