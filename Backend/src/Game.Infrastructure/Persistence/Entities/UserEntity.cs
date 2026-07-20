namespace Game.Infrastructure.Persistence.Entities;

// Conta de usuário (identidade persistente). Independente do acesso via cartão/token
// físico. Criada/carregada pelos fluxos de login (Google/e-mail) — ver épico 2.
public class UserEntity
{
    public Guid Id { get; set; }

    // Identificador do usuário no Google (claim 'sub' do ID token). Nulo para contas
    // criadas por outro provedor (ex.: e-mail). Único entre os não-nulos.
    public string? GoogleSubject { get; set; }

    public string Email { get; set; } = string.Empty;

    // Provedor da última autenticação: 'google' | 'email'.
    public string AuthProvider { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public List<UserConsentEntity> Consents { get; set; } = new();
}
