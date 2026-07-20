namespace Game.Application.Auth;

// Contrato publicado pela fatia-base (F0): identidade do requester extraída do JWT.
// Consumido por US8/US9/US12/US13. Retorna null para jogo anônimo (D1).
public interface ICurrentUser
{
    Guid? UserId { get; }
}
