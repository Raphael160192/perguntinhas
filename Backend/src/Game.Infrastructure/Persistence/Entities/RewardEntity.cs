namespace Game.Infrastructure.Persistence.Entities;

public class RewardEntity
{
    public Guid Id { get; set; }
    public Guid GameSessionId { get; set; }

    // Jogador que ganhou o prêmio (quem acertou a pergunta).
    public Guid PlayerId { get; set; }

    public string Text { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int TimeInSeconds { get; set; }
    public string? TemplateId { get; set; }
    public string? CatalogVersion { get; set; }
    public int? IntensityLevel { get; set; }
    public Guid? ActorPlayerId { get; set; }
    public Guid? ReceiverPlayerId { get; set; }
    public int? RoundNumber { get; set; }
    public string? ExecutionType { get; set; }
    public string? ExecutionValue { get; set; }
    public DateTime CreatedAt { get; set; }

    public GameSessionEntity? GameSession { get; set; }
}
