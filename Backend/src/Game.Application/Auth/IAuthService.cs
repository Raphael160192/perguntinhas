namespace Game.Application.Auth;

public class LoginResultDto
{
    public string Token { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
}

public interface IAuthService
{
    // Retorna null quando o ID token do Google é inválido.
    Task<LoginResultDto?> LoginWithGoogleAsync(string idToken);
}
