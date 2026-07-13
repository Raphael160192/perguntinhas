namespace Game.Infrastructure.Persistence.Entities;

public class GameSessionEntity
{
    public Guid Id { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Mode { get; set; } = "Local";
    public string? JoinCode { get; set; }

    // Lista de IDs de perguntas na ordem sorteada da partida (ex: "[12,3,45]").
    public string QuestionOrderJson { get; set; } = "[]";

    public Guid? WinnerPlayerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public List<GamePlayerEntity> Players { get; set; } = new();
    public List<GameAnswerEntity> Answers { get; set; } = new();
}
