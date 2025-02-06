using System.Net;
using System.Security.Claims;

namespace InvoiceService;

/// <summary>
/// Provides a scoped, accessible context for the current HTTP request.
/// This includes metadata about the client such as the User ID, roles, IP address.
/// The CurrentContext is built in the middleware and can be injected into any service that needs it.
/// </summary>
public class RequestContext
{
    /// <summary>
    /// The current HttpContext. Represents all HTTP-specific information about an individual HTTP request.
    /// </summary>
    public HttpContext HttpContext { get; private set; } = null!;

    /// <summary>
    /// The Id of the user who made the current request, from the HttpContext.
    /// </summary>
    public virtual Guid? UserId { get; set; }

    /// <summary>
    /// List of roles associated with the current user from the HttpContext.
    /// </summary>
    public List<string>? Roles { get; private set; }

    /// <summary>
    /// The client's IP address from the current HttpContext.
    /// </summary>
    public IPAddress? IpAddress { get; private set; }

    /// <summary>
    /// The user agent string from the current HttpContext. 
    /// This typically includes information such as the application type, operating system, software vendor, or software version of the requesting software user agent.
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Populates the CurrentContext properties based on the provided HttpContext.
    /// </summary>
    /// <param name="httpContext">The current HttpContext instance.</param>
    public void Build(HttpContext httpContext) {
        HttpContext = httpContext;
        IpAddress = GetIpAddress(httpContext);
        UserAgent = GetUserAgent(httpContext);

        SetUser(httpContext.User);
    }

    /// <summary>
    /// Extracts the user's unique identifier and role information from the <see cref="ClaimsPrincipal"/> instance, 
    /// and assigns them to the corresponding properties of the CurrentContext.
    /// </summary>
    /// <param name="user">The <see cref="ClaimsPrincipal"/> representing the current user.</param>
    private void SetUser(ClaimsPrincipal user) {
        if (user.Identity is ClaimsIdentity identity) {
            if (identity.Claims.Any()) {
                var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                UserId = userId is null
                    ? null
                    : Guid.Parse(userId);
                Roles = identity.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();
            }
        }
    }

    /// <summary>
    /// Retrieves the client's IP address from the HttpContext.
    /// This first attempts to get the IP address from the "X-Real-IP" header. 
    /// If this fails, it falls back to the RemoteIpAddress property of the HttpContext's Connection.
    /// </summary>
    /// <param name="httpContext">The current HttpContext instance.</param>
    /// <returns>The client's IP address, or null if it cannot be determined.</returns>
    private static IPAddress? GetIpAddress(HttpContext httpContext) {
        if (httpContext.Request.Headers.TryGetValue("X-Real-IP", out var header)) {
            var ipString = header.ToString();
            if (IPAddress.TryParse(ipString, out IPAddress? ip)) {
                return ip;
            }
        }

        return httpContext.Connection?.RemoteIpAddress;
    }

    /// <summary>
    /// Retrieves the UserAgent string from the HttpContext.
    /// This is extracted from the "User-Agent" header of the HTTP request.
    /// </summary>
    /// <param name="httpContext">The current HttpContext instance.</param>
    /// <returns>The UserAgent string, or null if it cannot be determined.</returns>
    private static string? GetUserAgent(HttpContext httpContext) {
        if (httpContext.Request.Headers.TryGetValue("User-Agent", out var header)) {
            return header.ToString();
        }

        return null;
    }
}