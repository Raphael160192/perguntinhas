using Game.Domain.Enums;

namespace Game.Domain.Entities;

public class PendingRoundResult
{
    public int RoundNumber { get; set; }
    public int QuestionId { get; set; }
    public int SelectedOptionIndex { get; set; }
    public int CorrectAnswerIndex { get; set; }
    public bool IsCorrect { get; set; }
    public Guid CurrentPlayerId { get; set; }
    public Guid PunishedPlayerId { get; set; }
    public ClothingItem? LostClothing { get; set; }
    public Reward? Reward { get; set; }
    public bool IsGameOver { get; set; }
    public Guid? WinnerPlayerId { get; set; }
}
