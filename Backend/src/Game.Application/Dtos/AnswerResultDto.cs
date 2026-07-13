namespace Game.Application.Dtos;

public class AnswerResultDto
{
    public bool IsCorrect { get; set; }
    public int CorrectAnswerIndex { get; set; }
    public PlayerDto CurrentPlayer { get; set; } = null!;
    public PlayerDto PunishedPlayer { get; set; } = null!;
    public string? LostClothing { get; set; }
    public string? Reward { get; set; }
    public RewardDto? RewardDetails { get; set; }
    public bool IsGameOver { get; set; }
    public PlayerDto? Winner { get; set; }
    public string Message { get; set; } = string.Empty;
    public GameStateDto State { get; set; } = null!;
}

public class SubmitAnswerServiceResult
{
    public AnswerResultDto Result { get; set; } = null!;
    public bool StateChanged { get; set; }
}

public class CreateGameRequestDto
{
    public string Player1Name { get; set; } = "Jogador 1";
    public string Player2Name { get; set; } = "Jogador 2";
}

public class CreateGameResultDto
{
    public Guid GameId { get; set; }
    public GameStateDto State { get; set; } = null!;
}

public class AnswerRequestDto
{
    public int SelectedOptionIndex { get; set; }
    public Guid? PlayerId { get; set; }
}

public class CreateRemoteGameRequestDto
{
    public string Player1Name { get; set; } = "Jogador 1";
}

public class CreateRemoteGameResultDto
{
    public Guid GameId { get; set; }
    public string JoinCode { get; set; } = string.Empty;
    public Guid PlayerId { get; set; }
    public GameStateDto State { get; set; } = null!;
}

public class JoinGameRequestDto
{
    public string JoinCode { get; set; } = string.Empty;
    public string PlayerName { get; set; } = "Jogador 2";
    public Guid? PlayerId { get; set; }
}

public class JoinGameResultDto
{
    public Guid GameId { get; set; }
    public Guid PlayerId { get; set; }

    // true quando o jogador já pertencia à sala (reentrada, não um join novo).
    public bool Rejoined { get; set; }

    public GameStateDto State { get; set; } = null!;
}
