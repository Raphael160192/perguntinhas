namespace Game.Infrastructure.Auth;

// Configuração do JWT próprio do backend. Vinculada à seção "Jwt" do appsettings;
// a SigningKey vem exclusivamente de variável de ambiente (nunca commitada).
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int LifetimeMinutes { get; set; } = 60;
    public string SigningKey { get; set; } = string.Empty;
}
