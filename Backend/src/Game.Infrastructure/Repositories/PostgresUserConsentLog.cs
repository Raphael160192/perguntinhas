using Game.Application.Repositories;
using Game.Domain.Entities;
using Game.Infrastructure.Persistence;
using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Infrastructure.Repositories;

// Trilha append-only de consentimentos (D3): só inserção, nunca update/delete.
public class PostgresUserConsentLog : IUserConsentLog
{
    private readonly ApplicationDbContext _dbContext;

    public PostgresUserConsentLog(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AppendAsync(Guid userId, string consentType, string policyVersion)
    {
        var entity = new UserConsentEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConsentType = consentType,
            PolicyVersion = policyVersion,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.UserConsents.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<UserConsent?> FindLatestAsync(Guid userId, string consentType)
    {
        var entity = await _dbContext.UserConsents
            .Where(c => c.UserId == userId && c.ConsentType == consentType)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        return entity is null ? null : ToDomain(entity);
    }

    private static UserConsent ToDomain(UserConsentEntity entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        ConsentType = entity.ConsentType,
        PolicyVersion = entity.PolicyVersion,
        CreatedAt = entity.CreatedAt
    };
}
