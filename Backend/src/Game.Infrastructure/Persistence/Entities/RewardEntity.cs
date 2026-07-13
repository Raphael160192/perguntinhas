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
    public DateTime CreatedAt { get; set; }

    public GameSessionEntity? GameSession { get; set; }
}
