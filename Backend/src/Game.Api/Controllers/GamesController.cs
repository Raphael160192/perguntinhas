using Game.Api.Contracts;
using Game.Application.Dtos;
using Game.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Game.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost]
    public ActionResult<CreateGameResultDto> CreateGame([FromBody] CreateGameRequest request)
    {
        var result = _gameService.CreateGame(new CreateGameRequestDto
        {
            Player1Name = request.Player1Name,
            Player2Name = request.Player2Name
        });

        return Ok(result);
    }

    [HttpGet("{gameId}")]
    public ActionResult<GameStateDto> GetState(Guid gameId)
    {
        var state = _gameService.GetState(gameId);
        return state is null ? NotFound() : Ok(state);
    }

    [HttpGet("{gameId}/question")]
    public ActionResult<QuestionDto> GetCurrentQuestion(Guid gameId)
    {
        var question = _gameService.GetCurrentQuestion(gameId);
        return question is null ? NotFound() : Ok(question);
    }

    [HttpPost("{gameId}/answer")]
    public ActionResult<AnswerResultDto> SubmitAnswer(Guid gameId, [FromBody] AnswerRequest request)
    {
        var result = _gameService.SubmitAnswer(gameId, new AnswerRequestDto
        {
            SelectedOptionIndex = request.SelectedOptionIndex
        });

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{gameId}/next")]
    public ActionResult<GameStateDto> NextRound(Guid gameId)
    {
        var state = _gameService.NextRound(gameId);
        return state is null ? NotFound() : Ok(state);
    }

    [HttpPost("{gameId}/restart")]
    public ActionResult<GameStateDto> Restart(Guid gameId)
    {
        var state = _gameService.Restart(gameId);
        return state is null ? NotFound() : Ok(state);
    }
}
