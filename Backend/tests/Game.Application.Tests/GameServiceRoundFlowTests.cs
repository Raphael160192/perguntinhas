using Game.Application.Dtos;
using Game.Application.Repositories;
using Game.Application.Rewards;
using Game.Application.Services;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Xunit;

namespace Game.Application.Tests;

public class GameServiceRoundFlowTests
{
    [Fact]
    public async Task SubmitAnswerAsync_RepeatedRequest_ReturnsStoredResultWithoutApplyingRulesAgain()
    {
        var session = CreateSession();
        var sessionRepository = new InMemorySessionRepository(session);
        var rewardSelector = new CountingRewardSelector();
        var activityLog = new CountingActivityLog();
        var service = CreateService(sessionRepository, rewardSelector, activityLog);

        var request = new AnswerRequestDto { SelectedOptionIndex = 1 };

        var first = await service.SubmitAnswerAsync(session.Id, request);
        var second = await service.SubmitAnswerAsync(session.Id, request);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.True(first.StateChanged);
        Assert.False(second.StateChanged);
        Assert.True(first.Result.IsCorrect);
        Assert.Equal(first.Result.RewardDetails?.TemplateId, second.Result.RewardDetails?.TemplateId);
        Assert.Equal(11, session.Players[1].Score);
        Assert.Equal(session.RoundNumber, session.AnsweredRoundNumber);
        Assert.Equal(1, sessionRepository.UpdateCount);
        Assert.Equal(1, rewardSelector.SelectionCount);
        Assert.Equal(1, activityLog.AnswerCount);
        Assert.Equal(1, activityLog.RewardCount);
    }

    [Fact]
    public async Task NextRoundAsync_BeforeAnswer_RejectsAdvance()
    {
        var session = CreateSession();
        var service = CreateService(
            new InMemorySessionRepository(session),
            new CountingRewardSelector(),
            new CountingActivityLog());

        var exception = await Assert.ThrowsAsync<GameRuleException>(
            () => service.NextRoundAsync(session.Id));

        Assert.Contains("Responda a pergunta", exception.Message);
        Assert.Equal(1, session.RoundNumber);
        Assert.Equal(0, session.CurrentPlayerIndex);
    }

    [Fact]
    public async Task NextRoundAsync_AfterAnswer_AdvancesExactlyOneRoundAndClearsPendingResult()
    {
        var session = CreateSession();
        var repository = new InMemorySessionRepository(session);
        var service = CreateService(
            repository,
            new CountingRewardSelector(),
            new CountingActivityLog());

        await service.SubmitAnswerAsync(
            session.Id,
            new AnswerRequestDto { SelectedOptionIndex = 0 });

        var state = await service.NextRoundAsync(session.Id);

        Assert.NotNull(state);
        Assert.Equal(2, state.RoundNumber);
        Assert.Equal(1, state.CurrentPlayerIndex);
        Assert.Null(state.PendingRoundResult);
        Assert.Equal(2, repository.UpdateCount);
    }

    [Fact]
    public async Task GetStateAsync_WithLegacyReward_PreservesTextForRefreshCompatibility()
    {
        var session = CreateSession();
        var service = CreateService(
            new InMemorySessionRepository(session),
            new LegacyLikeRewardSelector(),
            new CountingActivityLog());

        await service.SubmitAnswerAsync(
            session.Id,
            new AnswerRequestDto { SelectedOptionIndex = 1 });

        var state = await service.GetStateAsync(session.Id);

        Assert.NotNull(state?.PendingRoundResult);
        Assert.Equal("carícia na(o) testa por 5 segundos", state.PendingRoundResult.Reward);
        Assert.Null(state.PendingRoundResult.RewardDetails);
    }

    [Theory]
    [InlineData(GameStatus.InProgress, GameStatus.Abandoned)]
    [InlineData(GameStatus.Finished, GameStatus.Finished)]
    public async Task AbandonAsync_IdentifiesPlayerWhoLeft_DuringOrAfterGame(
        GameStatus initialStatus,
        GameStatus expectedStatus)
    {
        var session = CreateSession();
        session.Status = initialStatus;
        var leavingPlayer = session.Players[1];
        var service = CreateService(
            new InMemorySessionRepository(session),
            new CountingRewardSelector(),
            new CountingActivityLog());

        var result = await service.AbandonAsync(session.Id, leavingPlayer.Id);

        Assert.NotNull(result);
        Assert.Equal(leavingPlayer.Id, result.AbandonedByPlayerId);
        Assert.Equal(leavingPlayer.Name, result.AbandonedByName);
        Assert.Equal(expectedStatus.ToString(), result.State.Status);
    }

    private static GameService CreateService(
        IGameSessionRepository sessionRepository,
        IRewardSelector rewardSelector,
        IGameActivityLog activityLog) =>
        new(
            sessionRepository,
            new FixedQuestionRepository(),
            rewardSelector,
            activityLog);

    private static GameSession CreateSession() => new()
    {
        Players =
        [
            new Player { Name = "Jogador 1" },
            new Player { Name = "Jogador 2" }
        ],
        QuestionOrder = [1],
        CurrentPlayerIndex = 0,
        CurrentQuestionIndex = 0,
        RoundNumber = 1,
        Status = GameStatus.InProgress
    };

    private sealed class InMemorySessionRepository(GameSession session) : IGameSessionRepository
    {
        public int UpdateCount { get; private set; }

        public Task<GameSession> AddAsync(GameSession value) => Task.FromResult(value);

        public Task<GameSession?> GetAsync(Guid id) =>
            Task.FromResult<GameSession?>(id == session.Id ? session : null);

        public Task<GameSession?> GetByJoinCodeAsync(string joinCode) =>
            Task.FromResult<GameSession?>(null);

        public Task UpdateAsync(GameSession value)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedQuestionRepository : IQuestionRepository
    {
        private static readonly Question Question = new()
        {
            Id = 1,
            Text = "Pergunta",
            Options = ["Incorreta", "Correta"],
            CorrectAnswerIndex = 1,
            Level = 1,
            Theme = "Geral"
        };

        public Task<List<Question>> GetAllAsync() => Task.FromResult(new List<Question> { Question });

        public Task<Question?> GetByIdAsync(int id) =>
            Task.FromResult<Question?>(id == Question.Id ? Question : null);
    }

    private sealed class CountingRewardSelector : IRewardSelector
    {
        public int SelectionCount { get; private set; }

        public RewardSelectionResult Select(RewardSelectionContext context)
        {
            SelectionCount++;
            return new RewardSelectionResult
            {
                CurrentLevel = 1,
                TargetLevel = 1,
                CandidateCount = 1,
                Reward = new Reward
                {
                    TemplateId = "test.reward",
                    CatalogVersion = "test",
                    RenderedText = "Prêmio de teste",
                    IntensityLevel = 1,
                    ActorPlayerId = context.Actor.Id,
                    ReceiverPlayerId = context.Receiver.Id,
                    RoundNumber = context.Session.RoundNumber
                }
            };
        }
    }

    private sealed class CountingActivityLog : IGameActivityLog
    {
        public int AnswerCount { get; private set; }
        public int RewardCount { get; private set; }

        public Task RecordAnswerAsync(
            Guid gameSessionId,
            Guid playerId,
            int questionId,
            int selectedOptionIndex,
            bool isCorrect)
        {
            AnswerCount++;
            return Task.CompletedTask;
        }

        public Task RecordRewardAsync(Guid gameSessionId, Guid playerId, Reward reward)
        {
            RewardCount++;
            return Task.CompletedTask;
        }

        public Task RecordEventAsync(Guid gameSessionId, Guid? playerId, string eventType, object? payload = null)
        {
            EventCount++;
            EventTypes.Add(eventType);
            return Task.CompletedTask;
        }

        public int EventCount { get; private set; }
        public List<string> EventTypes { get; } = new();
    }

    private sealed class LegacyLikeRewardSelector : IRewardSelector
    {
        public RewardSelectionResult Select(RewardSelectionContext context) => new()
        {
            CurrentLevel = 1,
            TargetLevel = 1,
            CandidateCount = 1,
            Reward = new Reward
            {
                Action = "carícia",
                Location = "testa",
                TimeInSeconds = 5,
                ActorPlayerId = context.Actor.Id,
                ReceiverPlayerId = context.Receiver.Id,
                RoundNumber = context.Session.RoundNumber
            }
        };
    }
}
