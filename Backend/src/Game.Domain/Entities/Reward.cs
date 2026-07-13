using Game.Domain.Enums;

namespace Game.Domain.Entities;

public class Reward
{
    public string? TemplateId { get; set; }
    public string? CatalogVersion { get; set; }
    public string? RenderedText { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int TimeInSeconds { get; set; }
    public int IntensityLevel { get; set; } = 1;
    public RewardExecutionType ExecutionType { get; set; } = RewardExecutionType.Seconds;
    public string ExecutionValue { get; set; } = string.Empty;
    public Guid ActorPlayerId { get; set; }
    public Guid ReceiverPlayerId { get; set; }
    public int RoundNumber { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public string Text => RenderedText ?? $"{Action} na(o) {Location} por {TimeInSeconds} segundos";
}
