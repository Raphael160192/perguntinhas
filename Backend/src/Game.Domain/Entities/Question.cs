namespace Game.Domain.Entities;

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int CorrectAnswerIndex { get; set; }
    public int Level { get; set; }
    public string Theme { get; set; } = string.Empty;
}
