namespace Game.Application.Auth;

// Identidade extraída de um ID token válido do Google.
public record GoogleUserInfo(string Subject, string Email);

// Valida o ID token recebido do frontend (Google Identity Services).
// Retorna null para token inválido, expirado ou de audience errada.
public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo?> ValidateAsync(string idToken);
}
