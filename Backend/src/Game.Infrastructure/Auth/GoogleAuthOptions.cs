namespace Game.Infrastructure.Auth;

// Configuração do login com Google. ClientId vem de env var (GoogleAuth__ClientId)
// ou do appsettings — é o OAuth Client ID (Web) criado no Google Cloud Console.
public class GoogleAuthOptions
{
    public const string SectionName = "GoogleAuth";

    public string ClientId { get; set; } = string.Empty;
}
