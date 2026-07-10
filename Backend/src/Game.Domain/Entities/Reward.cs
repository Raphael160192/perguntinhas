namespace Game.Domain.Entities;

public class Reward
{
    public string Action { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int TimeInSeconds { get; set; }

    public string Text => $"{Action} na(o) {Location} por {TimeInSeconds} segundos";
}
