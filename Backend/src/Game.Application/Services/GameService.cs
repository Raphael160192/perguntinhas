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

    public CreateGameResultDto CreateGame(CreateGameRequestDto request)
    {
        var session = new GameSession
        {
            Players = new List<Player>
            {
                new() { Name = string.IsNullOrWhiteSpace(request.Player1Name) ? "Jogador 1" : request.Player1Name },
                new() { Name = string.IsNullOrWhiteSpace(request.Player2Name) ? "Jogador 2" : request.Player2Name }
            },
            QuestionOrder = ShuffleQuestionOrder(),
            CurrentPlayerIndex = 0,
            CurrentQuestionIndex = 0
        };

        _sessionRepository.Add(session);

        return new CreateGameResultDto
        {
            GameId = session.Id,
            State = ToStateDto(session)
        };
    }

    public GameStateDto? GetState(Guid gameId)
    {
        var session = _sessionRepository.Get(gameId);
        return session is null ? null : ToStateDto(session);
    }

    public QuestionDto? GetCurrentQuestion(Guid gameId)
    {
        var session = _sessionRepository.Get(gameId);
        if (session is null)
        {
            return null;
        }

        return ToQuestionDto(GetCurrentQuestionEntity(session));
    }

    public AnswerResultDto? SubmitAnswer(Guid gameId, AnswerRequestDto request)
    {
        var session = _sessionRepository.Get(gameId);
        if (session is null)
        {
            return null;
        }

        var question = GetCurrentQuestionEntity(session);
        bool isCorrect = request.SelectedOptionIndex == question.CorrectAnswerIndex;

        var outcome = GameRules.ApplyAnswer(session, isCorrect);

        Reward? reward = isCorrect ? _rewardProvider.GenerateRandomReward() : null;

        _sessionRepository.Update(session);

        return new AnswerResultDto
        {
            IsCorrect = outcome.IsCorrect,
            CurrentPlayer = ToPlayerDto(outcome.CurrentPlayer),
            PunishedPlayer = ToPlayerDto(outcome.PunishedPlayer),
            LostClothing = outcome.LostClothing?.ToString(),
            Reward = reward?.Text,
            IsGameOver = outcome.IsGameOver,
            Winner = outcome.Winner is null ? null : ToPlayerDto(outcome.Winner),
            Message = isCorrect ? "Correto!" : "Errado!",
            State = ToStateDto(session)
        };
    }

    public GameStateDto? NextRound(Guid gameId)
    {
        var session = _sessionRepository.Get(gameId);
        if (session is null)
        {
            return null;
        }

        if (session.Status == GameStatus.InProgress)
        {
            session.CurrentPlayerIndex = 1 - session.CurrentPlayerIndex;
            session.CurrentQuestionIndex = (session.CurrentQuestionIndex + 1) % session.QuestionOrder.Count;
        }

        _sessionRepository.Update(session);

        return ToStateDto(session);
    }

    public GameStateDto? Restart(Guid gameId)
    {
        var session = _sessionRepository.Get(gameId);
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

        session.QuestionOrder = ShuffleQuestionOrder();
        session.CurrentPlayerIndex = 0;
        session.CurrentQuestionIndex = 0;
        session.Status = GameStatus.InProgress;
        session.WinnerPlayerId = null;
        session.FinishedAt = null;

        _sessionRepository.Update(session);

        return ToStateDto(session);
    }

    private List<int> ShuffleQuestionOrder()
    {
        var questions = _questionRepository.GetAll();
        var indices = Enumerable.Range(0, questions.Count).ToList();
        var random = Random.Shared;

        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        return indices;
    }

    private Question GetCurrentQuestionEntity(GameSession session)
    {
        var questions = _questionRepository.GetAll();
        int questionId = session.QuestionOrder[session.CurrentQuestionIndex];
        return questions[questionId];
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

    private GameStateDto ToStateDto(GameSession session)
    {
        var winner = session.WinnerPlayerId.HasValue
            ? session.Players.FirstOrDefault(p => p.Id == session.WinnerPlayerId.Value)
            : null;

        return new GameStateDto
        {
            GameId = session.Id,
            Status = session.Status.ToString(),
            CurrentPlayerIndex = session.CurrentPlayerIndex,
            Players = session.Players.Select(ToPlayerDto).ToList(),
            CurrentQuestion = session.Status == GameStatus.InProgress
                ? ToQuestionDto(GetCurrentQuestionEntity(session))
                : null,
            WinnerPlayerId = session.WinnerPlayerId,
            WinnerName = winner?.Name,
            CreatedAt = session.CreatedAt,
            FinishedAt = session.FinishedAt
        };
    }
}
