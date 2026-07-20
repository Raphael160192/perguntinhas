namespace Game.Api.Contracts;

public class GoogleLoginRequest
{
    // ID token (credential) devolvido pelo Google Identity Services no frontend.
    public string IdToken { get; set; } = string.Empty;
}
