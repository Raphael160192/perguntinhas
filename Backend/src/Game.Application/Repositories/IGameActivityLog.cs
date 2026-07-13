using Game.Domain.Entities;

namespace Game.Application.Repositories;

// Registro append-only para análise (histórico de respostas e prêmios).
// Falhas aqui não devem interromper o fluxo do jogo.
public interface IGameActivityLog
{
    Task RecordAnswerAsync(Guid gameSessionId, Guid playerId, int questionId, int selectedOptionIndex, bool isCorrect);
    Task RecordRewardAsync(Guid gameSessionId, Guid playerId, Reward reward);
}
