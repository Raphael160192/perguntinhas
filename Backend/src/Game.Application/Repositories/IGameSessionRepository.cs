using Game.Domain.Entities;

namespace Game.Application.Repositories;

public interface IGameSessionRepository
{
    Task<GameSession> AddAsync(GameSession session);
    Task<GameSession?> GetAsync(Guid id);
    Task<GameSession?> GetByJoinCodeAsync(string joinCode);
    Task UpdateAsync(GameSession session);
}
