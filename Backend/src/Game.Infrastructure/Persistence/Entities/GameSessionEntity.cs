namespace Game.Infrastructure.Persistence.Entities;

public class GameSessionEntity
{
    public Guid Id { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? WinnerPlayerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public List<GamePlayerEntity> Players { get; set; } = new();
    public List<GameAnswerEntity> Answers { get; set; } = new();
}
