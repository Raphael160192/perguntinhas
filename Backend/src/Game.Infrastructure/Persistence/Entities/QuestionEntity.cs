namespace Game.Infrastructure.Persistence.Entities;

public class QuestionEntity
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public int Level { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<QuestionOptionEntity> Options { get; set; } = new();
}
