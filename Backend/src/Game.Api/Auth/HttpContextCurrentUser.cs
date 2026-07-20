using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Game.Application.Auth;

namespace Game.Api.Auth;

// Lê o UserId do claim 'sub' do JWT validado (quando presente). Aceita tanto o
// claim 'sub' cru (MapInboundClaims = false) quanto o NameIdentifier mapeado.
public class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid? UserId
    {
        get
        {
            var principal = _accessor.HttpContext?.User;
            var sub = principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }
}
