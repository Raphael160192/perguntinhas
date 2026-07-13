using Game.Api.Contracts;
using Game.Api.Hubs;
using Game.Application.Dtos;
using Game.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Game.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IHubContext<GameHub> _gameHub;

    public GamesController(IGameService gameService, IHubContext<GameHub> gameHub)
    {
        _gameService = gameService;
        _gameHub = gameHub;
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

    [HttpPost("remote")]
    public ActionResult<CreateRemoteGameResultDto> CreateRemoteGame([FromBody] CreateRemoteGameRequest request)
    {
        try
        {
            var result = _gameService.CreateRemoteGame(new CreateRemoteGameRequestDto
            {
                Player1Name = request.Player1Name
            });

            return Ok(result);
        }
        catch (GameRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("join")]
    public async Task<ActionResult<JoinGameResultDto>> JoinGame([FromBody] JoinGameRequest request)
    {
        try
        {
            var result = _gameService.JoinGame(new JoinGameRequestDto
            {
                JoinCode = request.JoinCode,
                PlayerName = request.PlayerName
            });

            await BroadcastAsync(result.GameId, "PlayerJoined", result.State);

            return Ok(result);
        }
        catch (GameRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
    public async Task<ActionResult<AnswerResultDto>> SubmitAnswer(Guid gameId, [FromBody] AnswerRequest request)
    {
        try
        {
            var result = _gameService.SubmitAnswer(gameId, new AnswerRequestDto
            {
                SelectedOptionIndex = request.SelectedOptionIndex,
                PlayerId = request.PlayerId
            });

            if (result is null)
            {
                return NotFound();
            }

            await BroadcastAsync(gameId, "AnswerSubmitted", result);

            return Ok(result);
        }
        catch (GameRuleException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{gameId}/next")]
    public async Task<ActionResult<GameStateDto>> NextRound(Guid gameId, [FromBody] NextRoundRequest? request = null)
    {
        try
        {
            var state = _gameService.NextRound(gameId, request?.PlayerId);

            if (state is null)
            {
                return NotFound();
            }

            await BroadcastAsync(gameId, "RoundAdvanced", state);

            return Ok(state);
        }
        catch (GameRuleException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{gameId}/restart")]
    public async Task<ActionResult<GameStateDto>> Restart(Guid gameId)
    {
        var state = _gameService.Restart(gameId);

        if (state is null)
        {
            return NotFound();
        }

        await BroadcastAsync(gameId, "GameRestarted", state);

        return Ok(state);
    }

    private Task BroadcastAsync(Guid gameId, string eventName, object payload)
    {
        return _gameHub.Clients.Group(gameId.ToString()).SendAsync(eventName, payload);
    }
}
