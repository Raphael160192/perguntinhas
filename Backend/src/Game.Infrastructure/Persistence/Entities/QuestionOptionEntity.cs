namespace Game.Infrastructure.Persistence.Entities;

public class QuestionOptionEntity
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public int OptionIndex { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    public QuestionEntity? Question { get; set; }
}
