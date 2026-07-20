using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Game.Infrastructure.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace Game.Infrastructure.Tests;

// Prova o contrato de identidade da fatia-base (F0): o token emitido pelo backend
// valida com a mesma chave e devolve o UserId no claim 'sub'. É esse round-trip
// emissão↔leitura que os fluxos de login (US6/US7) e o ICurrentUser (US8) usam.
public class JwtIssuerTests
{
    private static readonly JwtOptions Options = new()
    {
        Issuer = "perguntinhas",
        Audience = "perguntinhas-app",
        LifetimeMinutes = 60,
        SigningKey = "test-signing-key-with-at-least-32-bytes!!"
    };

    [Fact]
    public void Issue_ThenValidate_RoundTripsUserIdInSubClaim()
    {
        var userId = Guid.NewGuid();
        var issuer = new JwtIssuer(Microsoft.Extensions.Options.Options.Create(Options));

        var token = issuer.Issue(userId);

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Options.Issuer,
            ValidateAudience = true,
            ValidAudience = Options.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Options.SigningKey))
        };

        var principal = handler.ValidateToken(token, parameters, out _);

        var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        Assert.Equal(userId.ToString(), sub);
    }

    [Fact]
    public void Validate_WithWrongKey_Fails()
    {
        var issuer = new JwtIssuer(Microsoft.Extensions.Options.Options.Create(Options));
        var token = issuer.Issue(Guid.NewGuid());

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Options.Issuer,
            ValidateAudience = true,
            ValidAudience = Options.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("a-completely-different-signing-key-32b!!"))
        };

        Assert.ThrowsAny<SecurityTokenException>(
            () => handler.ValidateToken(token, parameters, out _));
    }
}
