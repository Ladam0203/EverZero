using System.Security.Claims;

namespace Messages;

public class AuthorizationResponse
{
    public IEnumerable<ClaimDto> ClaimDtos { get; set; }
    
    public AuthorizationResponse(IEnumerable<ClaimDto> claimDtos)
    {
        ClaimDtos = claimDtos;
    }
}