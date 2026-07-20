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
    public async Task<ActionResult<CreateGameResultDto>> CreateGame([FromBody] CreateGameRequest request)
    {
        try
        {
            var result = await _gameService.CreateGameAsync(new CreateGameRequestDto
            {
                Player1Name = request.Player1Name,
                Player2Name = request.Player2Name
            });

            return Ok(result);
        }
        catch (GameRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("remote")]
    public async Task<ActionResult<CreateRemoteGameResultDto>> CreateRemoteGame([FromBody] CreateRemoteGameRequest request)
    {
        try
        {
            var result = await _gameService.CreateRemoteGameAsync(new CreateRemoteGameRequestDto
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
            var result = await _gameService.JoinGameAsync(new JoinGameRequestDto
            {
                JoinCode = request.JoinCode,
                PlayerName = request.PlayerName,
                PlayerId = request.PlayerId
            });

            // Reentrada não altera o estado da partida; só o join novo notifica o criador.
            if (!result.Rejoined)
            {
                await BroadcastAsync(result.GameId, "PlayerJoined", result.State);
            }

            return Ok(result);
        }
        catch (GameRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{gameId}")]
    public async Task<ActionResult<GameStateDto>> GetState(Guid gameId)
    {
        var state = await _gameService.GetStateAsync(gameId);
        return state is null ? NotFound() : Ok(state);
    }

    [HttpGet("{gameId}/question")]
    public async Task<ActionResult<QuestionDto>> GetCurrentQuestion(Guid gameId)
    {
        var question = await _gameService.GetCurrentQuestionAsync(gameId);
        return question is null ? NotFound() : Ok(question);
    }

    [HttpPost("{gameId}/answer")]
    public async Task<ActionResult<AnswerResultDto>> SubmitAnswer(Guid gameId, [FromBody] AnswerRequest request)
    {
        try
        {
            var serviceResult = await _gameService.SubmitAnswerAsync(gameId, new AnswerRequestDto
            {
                SelectedOptionIndex = request.SelectedOptionIndex,
                PlayerId = request.PlayerId
            });

            if (serviceResult is null)
            {
                return NotFound();
            }

            if (serviceResult.StateChanged)
            {
                await BroadcastAsync(gameId, "AnswerSubmitted", serviceResult.Result);
            }

            return Ok(serviceResult.Result);
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
            var state = await _gameService.NextRoundAsync(gameId, request?.PlayerId);

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
        try
        {
            var state = await _gameService.RestartAsync(gameId);

            if (state is null)
            {
                return NotFound();
            }

            await BroadcastAsync(gameId, "GameRestarted", state);

            return Ok(state);
        }
        catch (GameRuleException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{gameId}/abandon")]
    public async Task<ActionResult<AbandonGameResultDto>> Abandon(Guid gameId, [FromBody] AbandonGameRequest? request = null)
    {
        try
        {
            var result = await _gameService.AbandonAsync(gameId, request?.PlayerId);

            if (result is null)
            {
                return NotFound();
            }

            // Avisa o outro aparelho que um jogador saiu, inclusive após o fim do jogo.
            await BroadcastAsync(gameId, "GameAbandoned", new
            {
                state = result.State,
                abandonedByPlayerId = result.AbandonedByPlayerId,
                abandonedByName = result.AbandonedByName
            });

            return Ok(result);
        }
        catch (GameRuleException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private Task BroadcastAsync(Guid gameId, string eventName, object payload)
    {
        return _gameHub.Clients.Group(gameId.ToString()).SendAsync(eventName, payload);
    }
}
