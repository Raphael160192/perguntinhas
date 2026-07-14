using System.Text.Json;
using Game.Application.Repositories;
using Game.Domain.Entities;
using Game.Infrastructure.Persistence;
using Game.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Logging;

namespace Game.Infrastructure.Repositories;

public class PostgresGameActivityLog : IGameActivityLog
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PostgresGameActivityLog> _logger;

    public PostgresGameActivityLog(
        ApplicationDbContext dbContext,
        ILogger<PostgresGameActivityLog> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task RecordAnswerAsync(Guid gameSessionId, Guid playerId, int questionId, int selectedOptionIndex, bool isCorrect)
    {
        try
        {
            _dbContext.GameAnswers.Add(new GameAnswerEntity
            {
                Id = Guid.NewGuid(),
                GameSessionId = gameSessionId,
                PlayerId = playerId,
                QuestionId = questionId,
                SelectedOptionIndex = selectedOptionIndex,
                IsCorrect = isCorrect,
                CreatedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Analytics nunca pode derrubar uma jogada.
            _logger.LogError(ex, "Falha ao registrar resposta da partida {GameId}.", gameSessionId);
        }
    }

    public async Task RecordRewardAsync(Guid gameSessionId, Guid playerId, Reward reward)
    {
        try
        {
            _dbContext.Rewards.Add(new RewardEntity
            {
                Id = Guid.NewGuid(),
                GameSessionId = gameSessionId,
                PlayerId = playerId,
                Text = reward.Text,
                Action = reward.Action,
                Location = reward.Location,
                TimeInSeconds = reward.TimeInSeconds,
                TemplateId = reward.TemplateId,
                CatalogVersion = reward.CatalogVersion,
                IntensityLevel = reward.IntensityLevel,
                ActorPlayerId = reward.ActorPlayerId == Guid.Empty ? null : reward.ActorPlayerId,
                ReceiverPlayerId = reward.ReceiverPlayerId == Guid.Empty ? playerId : reward.ReceiverPlayerId,
                RoundNumber = reward.RoundNumber == 0 ? null : reward.RoundNumber,
                ExecutionType = reward.ExecutionType.ToString(),
                ExecutionValue = string.IsNullOrWhiteSpace(reward.ExecutionValue) ? null : reward.ExecutionValue,
                CreatedAt = reward.GeneratedAt
            });

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao registrar prêmio da partida {GameId}.", gameSessionId);
        }
    }

    public async Task RecordEventAsync(Guid gameSessionId, Guid? playerId, string eventType, object? payload = null)
    {
        try
        {
            _dbContext.GameEvents.Add(new GameEventEntity
            {
                GameSessionId = gameSessionId,
                PlayerId = playerId,
                EventType = eventType,
                PayloadJson = payload is null ? "{}" : JsonSerializer.Serialize(payload),
                CreatedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao registrar evento {EventType} da partida {GameId}.", eventType, gameSessionId);
        }
    }
}
