using Game.Domain.Entities;

namespace Game.Application.Repositories;

// Contrato publicado pela fatia-base (F0): trilha append-only de consentimentos (D3).
// Consumido por US9 (grava no cadastro) e US12 (versionamento/re-aceite).
public interface IUserConsentLog
{
    Task AppendAsync(Guid userId, string consentType, string policyVersion);
    Task<UserConsent?> FindLatestAsync(Guid userId, string consentType);
}
