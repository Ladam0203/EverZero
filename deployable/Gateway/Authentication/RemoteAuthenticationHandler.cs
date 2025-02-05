using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Gateway.Authentication;

public class RemoteAuthenticationOptions : AuthenticationSchemeOptions
{
    public RemoteAuthenticationOptions()
    {
    }
}

public class RemoteAuthenticationHandler : AuthenticationHandler<RemoteAuthenticationOptions>
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    
    public RemoteAuthenticationHandler(
        IOptionsMonitor<RemoteAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IConfiguration config,
        HttpClient httpClient
        ) : base(options, logger, encoder, clock)
    {
       _config = config;
        _httpClient = httpClient;
    }
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader))
            return AuthenticateResult.Fail("Missing Authorization header.");

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, _config["RemoteAuthenticationUrl"]);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authHeader.Replace("Bearer ", ""));

        var response = await _httpClient.SendAsync(requestMessage);
        if (!response.IsSuccessStatusCode)
            return AuthenticateResult.Fail("Invalid token.");

        var authResult = await response.Content.ReadFromJsonAsync<AuthorizationResponse>();
        if (authResult == null || !authResult.IsAuthenticated)
            return AuthenticateResult.Fail("Unauthorized.");

        var claims = authResult.Claims.Select(c => new Claim(c.Type, c.Value)).ToArray();
        var claimsIdentity = new ClaimsIdentity(claims, Scheme.Name);
        var claimPrincipal = new ClaimsPrincipal(claimsIdentity);
        var authenticationTicket = new AuthenticationTicket(claimPrincipal, Scheme.Name);

        return AuthenticateResult.Success(authenticationTicket);
    }
}

public class AuthorizationResponse
{
    public bool IsAuthenticated { get; set; }
    public Claim[] Claims { get; set; }
}