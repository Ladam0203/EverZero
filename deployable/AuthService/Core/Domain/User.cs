using Microsoft.AspNetCore.Identity;

namespace AuthService.Identity;

public class User : IdentityUser<Guid>
{
    /// <summary>
    /// Whether the user is active or not.
    /// </summary>
    public bool IsActive { get; set; }
}