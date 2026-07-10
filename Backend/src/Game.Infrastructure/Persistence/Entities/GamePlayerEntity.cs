namespace Game.Infrastructure.Persistence.Entities;

public class GamePlayerEntity
{
    public Guid Id { get; set; }
    public Guid GameSessionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool Socks { get; set; }
    public bool Shirt { get; set; }
    public bool Pants { get; set; }
    public bool Underwear { get; set; }

    // Lista de pontuações em que o jogador já perdeu peça, serializada como JSON (ex: "[9,6]").
    public string ClothingLostAtScoresJson { get; set; } = "[]";

    public GameSessionEntity? GameSession { get; set; }
}
