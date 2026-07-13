using Game.Domain.Enums;

namespace Game.Domain.Entities;

public class GameSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public List<Player> Players { get; set; } = new();
    public int CurrentPlayerIndex { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public List<int> QuestionOrder { get; set; } = new();
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public GameMode Mode { get; set; } = GameMode.Local;
    public string? JoinCode { get; set; }
    public Guid? WinnerPlayerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }

    public Player CurrentPlayer => Players[CurrentPlayerIndex];
    public Player OpponentPlayer => Players[1 - CurrentPlayerIndex];
}
