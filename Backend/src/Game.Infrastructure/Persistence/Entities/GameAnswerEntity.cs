namespace Game.Infrastructure.Persistence.Entities;

public class GameAnswerEntity
{
    public Guid Id { get; set; }
    public Guid GameSessionId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid QuestionId { get; set; }
    public int SelectedOptionIndex { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime CreatedAt { get; set; }

    public GameSessionEntity? GameSession { get; set; }
}
