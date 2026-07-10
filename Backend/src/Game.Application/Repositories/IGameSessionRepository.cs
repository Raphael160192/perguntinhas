using Game.Domain.Entities;

namespace Game.Application.Repositories;

public interface IGameSessionRepository
{
    GameSession Add(GameSession session);
    GameSession? Get(Guid id);
    void Update(GameSession session);
}
