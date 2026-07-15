namespace Game.Domain.Enums;

public enum GameStatus
{
    WaitingForOpponent,
    InProgress,
    Finished,

    // Encerrada por um dos jogadores antes do fim (vale para os dois aparelhos).
    Abandoned
}
