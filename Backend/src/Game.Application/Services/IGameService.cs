using Game.Application.Dtos;

namespace Game.Application.Services;

public interface IGameService
{
    CreateGameResultDto CreateGame(CreateGameRequestDto request);
    CreateRemoteGameResultDto CreateRemoteGame(CreateRemoteGameRequestDto request);
    JoinGameResultDto JoinGame(JoinGameRequestDto request);
    GameStateDto? GetState(Guid gameId);
    QuestionDto? GetCurrentQuestion(Guid gameId);
    AnswerResultDto? SubmitAnswer(Guid gameId, AnswerRequestDto request);
    GameStateDto? NextRound(Guid gameId, Guid? playerId = null);
    GameStateDto? Restart(Guid gameId);
}
