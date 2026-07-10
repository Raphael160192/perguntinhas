namespace Game.Domain.Entities;

public class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; } = 12;
    public ClothingState Clothes { get; set; } = new();
    public List<int> ClothingLostAtScores { get; set; } = new();
}
