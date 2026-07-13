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

public class RewardDto
{
    public string TemplateId { get; set; } = string.Empty;
    public string CatalogVersion { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Level { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public Guid ActorPlayerId { get; set; }
    public Guid ReceiverPlayerId { get; set; }
    public string ExecutionType { get; set; } = string.Empty;
    public string ExecutionValue { get; set; } = string.Empty;
}

public class PendingRoundResultDto
{
    public int RoundNumber { get; set; }
    public int CorrectAnswerIndex { get; set; }
    public bool IsCorrect { get; set; }
    public Guid CurrentPlayerId { get; set; }
    public Guid PunishedPlayerId { get; set; }
    public string? LostClothing { get; set; }
    public string? Reward { get; set; }
    public RewardDto? RewardDetails { get; set; }
    public bool IsGameOver { get; set; }
    public Guid? WinnerPlayerId { get; set; }
}

public class GameStateDto
{
    public Guid GameId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string? JoinCode { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public int RoundNumber { get; set; }
    public List<PlayerDto> Players { get; set; } = new();
    public QuestionDto? CurrentQuestion { get; set; }
    public PendingRoundResultDto? PendingRoundResult { get; set; }
    public Guid? WinnerPlayerId { get; set; }
    public string? WinnerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
