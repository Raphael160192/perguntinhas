namespace Game.Application.Dtos;

public class ClothingStateDto
{
    public bool Socks { get; set; }
    public bool Shirt { get; set; }
    public bool Pants { get; set; }
    public bool Underwear { get; set; }
}

public class PlayerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public ClothingStateDto Clothes { get; set; } = new();
    public int RemainingClothesCount { get; set; }
}

public class QuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int Level { get; set; }
    public string Theme { get; set; } = string.Empty;
}

public class GameStateDto
{
    public Guid GameId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CurrentPlayerIndex { get; set; }
    public List<PlayerDto> Players { get; set; } = new();
    public QuestionDto? CurrentQuestion { get; set; }
    public Guid? WinnerPlayerId { get; set; }
    public string? WinnerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
