using Game.Application.Dtos;
using Game.Application.Repositories;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Domain.Rules;

namespace Game.Application.Services;

public class GameService : IGameService
{
    private readonly IGameSessionRepository _sessionRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IRewardProvider _rewardProvider;

    public GameService(
        IGameSessionRepository sessionRepository,
        IQuestionRepository questionRepository,
        IRewardProvider rewardProvider)
    {
        _sessionRepository = sessionRepository;
        _questionRepository = questionRepository;
        _rewardProvider = rewardProvider;
    }

    public async Task<CreateGameResultDto> CreateGameAsync(CreateGameRequestDto request)
    {
        var session = new GameSession
        {
            Players = new List<Player>
            {
                new() { Name = string.IsNullOrWhiteSpace(request.Player1Name) ? "Jogador 1" : request.Player1Name },
                new() { Name = string.IsNullOrWhiteSpace(request.Player2Name) ? "Jogador 2" : request.Player2Name }
            },
            QuestionOrder = await ShuffleQuestionOrderAsync(),
            CurrentPlayerIndex = 0,
            CurrentQuestionIndex = 0
        };

        await _sessionRepository.AddAsync(session);

        return new CreateGameResultDto
        {
            GameId = session.Id,
            State = await ToStateDtoAsync(session)
        };
    }

    public async Task<CreateRemoteGameResultDto> CreateRemoteGameAsync(CreateRemoteGameRequestDto request)
    {
        var session = new GameSession
        {
            Mode = GameMode.Remote,
            Status = GameStatus.WaitingForOpponent,
            JoinCode = await GenerateUniqueJoinCodeAsync(),
            Players = new List<Player>
            {
                new() { Name = string.IsNullOrWhiteSpace(request.Player1Name) ? "Jogador 1" : request.Player1Name }
            },
            QuestionOrder = await ShuffleQuestionOrderAsync(),
            CurrentPlayerIndex = 0,
            CurrentQuestionIndex = 0
        };

        await _sessionRepository.AddAsync(session);

        return new CreateRemoteGameResultDto
        {
            GameId = session.Id,
            JoinCode = session.JoinCode!,
            PlayerId = session.Players[0].Id,
            State = await ToStateDtoAsync(session)
        };
    }

    public async Task<JoinGameResultDto> JoinGameAsync(JoinGameRequestDto request)
    {
        var session = await _sessionRepository.GetByJoinCodeAsync(request.JoinCode?.Trim() ?? string.Empty);

        if (session is null)
        {
            throw new GameRuleException("Sala não encontrada. Confira o código.");
        }

        if (session.Status != GameStatus.WaitingForOpponent || session.Players.Count >= 2)
        {
            throw new GameRuleException("Esta sala já está cheia.");
        }

        var player2 = new Player
        {
            Name = string.IsNullOrWhiteSpace(request.PlayerName) ? "Jogador 2" : request.PlayerName
        };

        session.Players.Add(player2);
        session.Status = GameStatus.InProgress;

        await _sessionRepository.UpdateAsync(session);

        return new JoinGameResultDto
        {
            GameId = session.Id,
            PlayerId = player2.Id,
            State = await ToStateDtoAsync(session)
        };
    }

    public async Task<GameStateDto?> GetStateAsync(Guid gameId)
    {
        var session = await _sessionRepository.GetAsync(gameId);
        return session is null ? null : await ToStateDtoAsync(session);
    }

    public async Task<QuestionDto?> GetCurrentQuestionAsync(Guid gameId)
    {
        var session = await _sessionRepository.GetAsync(gameId);
        if (session is null)
        {
            return null;
        }

        return ToQuestionDto(await GetCurrentQuestionEntityAsync(session));
    }

    public async Task<AnswerResultDto?> SubmitAnswerAsync(Guid gameId, AnswerRequestDto request)
    {
        var session = await _sessionRepository.GetAsync(gameId);
        if (session is null)
        {
            return null;
        }

        EnsureIsCurrentPlayer(session, request.PlayerId);

        if (session.Status != GameStatus.InProgress)
        {
            throw new GameRuleException("A partida não está em andamento.");
        }

        var question = await GetCurrentQuestionEntityAsync(session);
        bool isCorrect = request.SelectedOptionIndex == question.CorrectAnswerIndex;

        var outcome = GameRules.ApplyAnswer(session, isCorrect);

        Reward? reward = isCorrect ? _rewardProvider.GenerateRandomReward() : null;

        await _sessionRepository.UpdateAsync(session);

        return new AnswerResultDto
        {
            IsCorrect = outcome.IsCorrect,
            CorrectAnswerIndex = question.CorrectAnswerIndex,
            CurrentPlayer = ToPlayerDto(outcome.CurrentPlayer),
            PunishedPlayer = ToPlayerDto(outcome.PunishedPlayer),
            LostClothing = outcome.LostClothing?.ToString(),
            Reward = reward?.Text,
            IsGameOver = outcome.IsGameOver,
            Winner = outcome.Winner is null ? null : ToPlayerDto(outcome.Winner),
            Message = isCorrect ? "Correto!" : "Errado!",
            State = await ToStateDtoAsync(session)
        };
    }

    public async Task<GameStateDto?> NextRoundAsync(Guid gameId, Guid? playerId = null)
    {
        var session = await _sessionRepository.GetAsync(gameId);
        if (session is null)
        {
            return null;
        }

        EnsureIsCurrentPlayer(session, playerId);

        if (session.Status == GameStatus.InProgress)
        {
            session.CurrentPlayerIndex = 1 - session.CurrentPlayerIndex;
            session.CurrentQuestionIndex = (session.CurrentQuestionIndex + 1) % session.QuestionOrder.Count;
        }

        await _sessionRepository.UpdateAsync(session);

        return await ToStateDtoAsync(session);
    }

    public async Task<GameStateDto?> RestartAsync(Guid gameId)
    {
        var session = await _sessionRepository.GetAsync(gameId);
        if (session is null)
        {
            return null;
        }

        foreach (var player in session.Players)
        {
            player.Score = 12;
            player.Clothes = new();
            player.ClothingLostAtScores.Clear();
        }

        session.QuestionOrder = await ShuffleQuestionOrderAsync();
        session.CurrentPlayerIndex = 0;
        session.CurrentQuestionIndex = 0;
        session.Status = GameStatus.InProgress;
        session.WinnerPlayerId = null;
        session.FinishedAt = null;

        await _sessionRepository.UpdateAsync(session);

        return await ToStateDtoAsync(session);
    }

    // Em partidas remotas, apenas o jogador da vez pode responder/avançar.
    // Partidas locais (um aparelho) não enviam playerId e não são validadas.
    private static void EnsureIsCurrentPlayer(GameSession session, Guid? playerId)
    {
        if (session.Mode != GameMode.Remote || playerId is null)
        {
            return;
        }

        if (session.Status == GameStatus.InProgress && session.CurrentPlayer.Id != playerId.Value)
        {
            throw new GameRuleException("Não é a sua vez.");
        }
    }

    // Alfabeto sem caracteres ambíguos (0/O, 1/I/L).
    private const string JoinCodeAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    private async Task<string> GenerateUniqueJoinCodeAsync()
    {
        for (int attempt = 0; attempt < 50; attempt++)
        {
            var code = new string(Enumerable.Range(0, 4)
                .Select(_ => JoinCodeAlphabet[Random.Shared.Next(JoinCodeAlphabet.Length)])
                .ToArray());

            if (await _sessionRepository.GetByJoinCodeAsync(code) is null)
            {
                return code;
            }
        }

        throw new GameRuleException("Não foi possível gerar um código de sala. Tente novamente.");
    }

    // Sorteia a ordem das perguntas como lista de IDs (estável mesmo se o catálogo mudar).
    private async Task<List<int>> ShuffleQuestionOrderAsync()
    {
        var questions = await _questionRepository.GetAllAsync();

        if (questions.Count == 0)
        {
            throw new GameRuleException("Não há perguntas cadastradas para iniciar uma partida.");
        }

        var ids = questions.Select(q => q.Id).ToList();
        var random = Random.Shared;

        for (int i = ids.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (ids[i], ids[j]) = (ids[j], ids[i]);
        }

        return ids;
    }

    private async Task<Question> GetCurrentQuestionEntityAsync(GameSession session)
    {
        int questionId = session.QuestionOrder[session.CurrentQuestionIndex];
        var question = await _questionRepository.GetByIdAsync(questionId);

        // Pergunta desativada/removida no meio da partida: pula para a próxima disponível.
        if (question is null)
        {
            var questions = await _questionRepository.GetAllAsync();
            question = questions.FirstOrDefault(q => session.QuestionOrder.Contains(q.Id))
                ?? throw new GameRuleException("Não há perguntas disponíveis para esta partida.");
        }

        return question;
    }

    private static QuestionDto ToQuestionDto(Question question) => new()
    {
        Id = question.Id,
        Text = question.Text,
        Options = question.Options,
        Level = question.Level,
        Theme = question.Theme
    };

    private static PlayerDto ToPlayerDto(Player player) => new()
    {
        Id = player.Id,
        Name = player.Name,
        Score = player.Score,
        Clothes = new ClothingStateDto
        {
            Socks = player.Clothes.Socks,
            Shirt = player.Clothes.Shirt,
            Pants = player.Clothes.Pants,
            Underwear = player.Clothes.Underwear
        },
        RemainingClothesCount = player.Clothes.RemainingCount()
    };

    private async Task<GameStateDto> ToStateDtoAsync(GameSession session)
    {
        var winner = session.WinnerPlayerId.HasValue
            ? session.Players.FirstOrDefault(p => p.Id == session.WinnerPlayerId.Value)
            : null;

        return new GameStateDto
        {
            GameId = session.Id,
            Status = session.Status.ToString(),
            Mode = session.Mode.ToString(),
            JoinCode = session.JoinCode,
            CurrentPlayerIndex = session.CurrentPlayerIndex,
            Players = session.Players.Select(ToPlayerDto).ToList(),
            CurrentQuestion = session.Status == GameStatus.InProgress
                ? ToQuestionDto(await GetCurrentQuestionEntityAsync(session))
                : null,
            WinnerPlayerId = session.WinnerPlayerId,
            WinnerName = winner?.Name,
            CreatedAt = session.CreatedAt,
            FinishedAt = session.FinishedAt
        };
    }
}
