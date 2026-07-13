using System.Text.Json;
using Game.Application.Repositories;
using Game.Application.Services;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Infrastructure.Persistence;
using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Game.Infrastructure.Repositories;

public class PostgresGameSessionRepository : IGameSessionRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PostgresGameSessionRepository> _logger;

    public PostgresGameSessionRepository(
        ApplicationDbContext dbContext,
        ILogger<PostgresGameSessionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<GameSession> AddAsync(GameSession session)
    {
        var entity = new GameSessionEntity
        {
            Id = session.Id,
            CreatedAt = session.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        CopyToEntity(session, entity);

        _dbContext.GameSessions.Add(entity);
        await _dbContext.SaveChangesAsync();

        return session;
    }

    public async Task<GameSession?> GetAsync(Guid id)
    {
        var entity = await LoadAsync(s => s.Id == id);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<GameSession?> GetByJoinCodeAsync(string joinCode)
    {
        if (string.IsNullOrWhiteSpace(joinCode))
        {
            return null;
        }

        var normalized = joinCode.Trim().ToUpperInvariant();
        var entity = await LoadAsync(s => s.JoinCode == normalized);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task UpdateAsync(GameSession session)
    {
        var entity = await _dbContext.GameSessions
            .Include(s => s.Players)
            .FirstOrDefaultAsync(s => s.Id == session.Id)
            ?? throw new GameRuleException("Partida não encontrada.");

        CopyToEntity(session, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        // Jogador novo (join na sala remota) ainda não existe na entidade.
        foreach (var player in session.Players)
        {
            if (entity.Players.All(p => p.Id != player.Id))
            {
                var playerEntity = new GamePlayerEntity
                {
                    Id = player.Id,
                    GameSessionId = entity.Id,
                    PlayerIndex = session.Players.IndexOf(player)
                };

                entity.Players.Add(playerEntity);

                // Marca explicitamente como INSERT: como o Id já vem preenchido, o EF
                // assumiria que a entidade existe no banco e emitiria um UPDATE fantasma
                // ("expected 1 row, affected 0").
                _dbContext.GamePlayers.Add(playerEntity);
            }
        }

        foreach (var playerEntity in entity.Players)
        {
            var player = session.Players.FirstOrDefault(p => p.Id == playerEntity.Id);
            if (player is not null)
            {
                CopyToEntity(player, playerEntity);
            }
        }

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Join duplo: duas pessoas entrando com o mesmo código ao mesmo tempo.
            // O índice único (GameSessionId, PlayerIndex) barra o segundo insert.
            _logger.LogWarning(ex, "Join simultâneo na partida {GameId}.", session.Id);
            throw new GameRuleException("Esta sala já está cheia.");
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException is Npgsql.PostgresException pg
            && pg.SqlState == Npgsql.PostgresErrorCodes.UniqueViolation;
    }

    private Task<GameSessionEntity?> LoadAsync(System.Linq.Expressions.Expression<Func<GameSessionEntity, bool>> predicate)
    {
        return _dbContext.GameSessions
            .Include(s => s.Players)
            .FirstOrDefaultAsync(predicate);
    }

    private GameSession ToDomain(GameSessionEntity entity)
    {
        var session = new GameSession
        {
            Id = entity.Id,
            CurrentPlayerIndex = entity.CurrentPlayerIndex,
            CurrentQuestionIndex = entity.CurrentQuestionIndex,
            QuestionOrder = JsonSerializer.Deserialize<List<int>>(entity.QuestionOrderJson) ?? new(),
            Status = Enum.Parse<GameStatus>(entity.Status),
            Mode = Enum.Parse<GameMode>(entity.Mode),
            JoinCode = entity.JoinCode,
            WinnerPlayerId = entity.WinnerPlayerId,
            CreatedAt = entity.CreatedAt,
            FinishedAt = entity.FinishedAt,
            Players = entity.Players
                .OrderBy(p => p.PlayerIndex)
                .Select(p => new Player
                {
                    Id = p.Id,
                    Name = p.Name,
                    Score = p.Score,
                    Clothes = new ClothingState
                    {
                        Socks = p.Socks,
                        Shirt = p.Shirt,
                        Pants = p.Pants,
                        Underwear = p.Underwear
                    },
                    ClothingLostAtScores =
                        JsonSerializer.Deserialize<List<int>>(p.ClothingLostAtScoresJson) ?? new()
                })
                .ToList()
        };

        return session;
    }

    private static void CopyToEntity(GameSession session, GameSessionEntity entity)
    {
        entity.CurrentPlayerIndex = session.CurrentPlayerIndex;
        entity.CurrentQuestionIndex = session.CurrentQuestionIndex;
        entity.QuestionOrderJson = JsonSerializer.Serialize(session.QuestionOrder);
        entity.Status = session.Status.ToString();
        entity.Mode = session.Mode.ToString();
        entity.JoinCode = session.JoinCode;
        entity.WinnerPlayerId = session.WinnerPlayerId;
        entity.FinishedAt = session.FinishedAt;

        if (entity.Players.Count == 0 && session.Players.Count > 0)
        {
            entity.Players = session.Players
                .Select((player, index) =>
                {
                    var playerEntity = new GamePlayerEntity
                    {
                        Id = player.Id,
                        GameSessionId = entity.Id,
                        PlayerIndex = index
                    };
                    CopyToEntity(player, playerEntity);
                    return playerEntity;
                })
                .ToList();
        }
    }

    private static void CopyToEntity(Player player, GamePlayerEntity entity)
    {
        entity.Name = player.Name;
        entity.Score = player.Score;
        entity.Socks = player.Clothes.Socks;
        entity.Shirt = player.Clothes.Shirt;
        entity.Pants = player.Clothes.Pants;
        entity.Underwear = player.Clothes.Underwear;
        entity.ClothingLostAtScoresJson = JsonSerializer.Serialize(player.ClothingLostAtScores);
    }
}
