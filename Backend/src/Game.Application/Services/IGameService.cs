using Game.Application.Dtos;

namespace Game.Application.Services;

public interface IGameService
{
    Task<CreateGameResultDto> CreateGameAsync(CreateGameRequestDto request);
    Task<CreateRemoteGameResultDto> CreateRemoteGameAsync(CreateRemoteGameRequestDto request);
    Task<JoinGameResultDto> JoinGameAsync(JoinGameRequestDto request);
    Task<GameStateDto?> GetStateAsync(Guid gameId);
    Task<QuestionDto?> GetCurrentQuestionAsync(Guid gameId);
    Task<SubmitAnswerServiceResult?> SubmitAnswerAsync(Guid gameId, AnswerRequestDto request);
    Task<GameStateDto?> NextRoundAsync(Guid gameId, Guid? playerId = null);
    Task<GameStateDto?> RestartAsync(Guid gameId);
}
