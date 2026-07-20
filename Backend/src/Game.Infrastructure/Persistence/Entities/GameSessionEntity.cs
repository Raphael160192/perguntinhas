namespace Game.Infrastructure.Persistence.Entities;

public class GameSessionEntity
{
    public Guid Id { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public int CurrentQuestionIndex { get; set; }
    public int RoundNumber { get; set; } = 1;
    public int? AnsweredRoundNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Mode { get; set; } = "Local";
    public string? JoinCode { get; set; }

    // Identidade opcional (D4): partidas anônimas ficam com UserId nulo; o vínculo
    // à conta é gravado pela US8. AccessChannel separa estruturalmente as trilhas
    // de dado ('anonymous' | 'authenticated' | futuro 'token') — nunca somadas.
    public Guid? UserId { get; set; }
    public string AccessChannel { get; set; } = "anonymous";

    // Lista de IDs de perguntas na ordem sorteada da partida (ex: "[12,3,45]").
    public string QuestionOrderJson { get; set; } = "[]";
    public string RewardProgressionJson { get; set; } = "{\"currentLevel\":1,\"rewardsGeneratedInCurrentStage\":0,\"recentRewards\":[]}";
    public string? PendingRoundResultJson { get; set; }
    public long Version { get; set; }

    public Guid? WinnerPlayerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public List<GamePlayerEntity> Players { get; set; } = new();
    public List<GameAnswerEntity> Answers { get; set; } = new();
}
