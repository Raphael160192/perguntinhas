namespace Game.Infrastructure.Persistence.Entities;

public class RewardEntity
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int TimeInSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
}
