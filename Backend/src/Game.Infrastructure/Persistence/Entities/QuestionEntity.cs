namespace Game.Infrastructure.Persistence.Entities;

public class QuestionEntity
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public int Level { get; set; }
    public bool Active { get; set; } = true;

    public List<QuestionOptionEntity> Options { get; set; } = new();
}
