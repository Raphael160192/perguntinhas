namespace Game.Infrastructure.Persistence.Entities;

// Registro de consentimento LGPD (D3). Tabela append-only: cada aceite gera uma
// linha nova, nunca update/delete. Uma nova versão da política exige novo aceite.
public class UserConsentEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Tipo do aceite: 'privacidade' | 'maioridade' | 'dados-sensiveis'.
    public string ConsentType { get; set; } = string.Empty;

    // Versão da política de privacidade vigente no momento do aceite (ex.: "v1").
    public string PolicyVersion { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public UserEntity? User { get; set; }
}
