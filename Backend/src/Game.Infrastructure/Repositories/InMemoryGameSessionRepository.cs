using System.Collections.Concurrent;
using Game.Application.Repositories;
using Game.Domain.Entities;

namespace Game.Infrastructure.Repositories;

public class InMemoryGameSessionRepository : IGameSessionRepository
{
    private readonly ConcurrentDictionary<Guid, GameSession> _sessions = new();

    public GameSession Add(GameSession session)
    {
        _sessions[session.Id] = session;
        return session;
    }

    public GameSession? Get(Guid id)
    {
        return _sessions.TryGetValue(id, out var session) ? session : null;
    }

    public GameSession? GetByJoinCode(string joinCode)
    {
        return _sessions.Values.FirstOrDefault(s =>
            string.Equals(s.JoinCode, joinCode, StringComparison.OrdinalIgnoreCase));
    }

    public void Update(GameSession session)
    {
        _sessions[session.Id] = session;
    }
}
