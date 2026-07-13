namespace Game.Api.Contracts;

public class AnswerRequest
{
    public int SelectedOptionIndex { get; set; }
    public Guid? PlayerId { get; set; }
}
