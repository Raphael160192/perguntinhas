namespace Game.Application.Auth;

// Contrato publicado pela fatia-base (F0): emite o JWT próprio do backend (D6),
// com o claim 'sub' = UserId interno. Consumido pelos fluxos de login US6/US7.
public interface IJwtIssuer
{
    string Issue(Guid userId);
}
