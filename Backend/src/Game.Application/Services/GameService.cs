using Game.Application.Dtos;
using Game.Application.Repositories;
using Game.Application.Rewards;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Domain.Rules;

namespace Game.Application.Services;

public class GameService : IGameService
{
    private readonly IGameSessionRepository _sessionRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IRewardSelector _rewardSelector;
    private readonly IGameActivityLog _activityLog;

    public GameService(
        IGameSessionRepository sessionRepository,
        IQuestionRepository questionRepository,
        IRewardSelector rewardSelector,
        IGameActivityLog activityLog)
    {
        _sessionRepository = sessionRepository;
        _questionRepository = questionRepository;
        _rewardSelector = rewardSelector;
        _activityLog = activityLog;
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

        await _activityLog.RecordEventAsync(session.Id, null, GameEventTypes.GameCreated, new
        {
            mode = session.Mode.ToString(),
            playerNames = session.Players.Select(p => p.Name).ToArray()
        });

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

        await _activityLog.RecordEventAsync(session.Id, session.Players[0].Id, GameEventTypes.GameCreated, new
        {
            mode = session.Mode.ToString(),
            joinCode = session.JoinCode,
            playerNames = session.Players.Select(p => p.Name).ToArray()
        });

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

        // Rejoin idempotente: quem já é jogador desta sala volta para ela em vez
        // de receber "sala cheia" (refresh, retorno acidental à home etc.).
        if (request.PlayerId.HasValue)
        {
            var existingPlayer = session.Players.FirstOrDefault(p => p.Id == request.PlayerId.Value);
            if (existingPlayer is not null)
            {
                return new JoinGameResultDto
                {
                    GameId = session.Id,
                    PlayerId = existingPlayer.Id,
                    Rejoined = true,
                    State = await ToStateDtoAsync(session)
                };
            }
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

        // Só o join novo gera evento — o rejoin retorna antes deste ponto.
        await _activityLog.RecordEventAsync(session.Id, player2.Id, GameEventTypes.PlayerJoined, new
        {
            playerName = player2.Name
        });

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

    public async Task<SubmitAnswerServiceResult?> SubmitAnswerAsync(Guid gameId, AnswerRequestDto request)
    {
        var session = await _sessionRepository.GetAsync(gameId);
        if (session is null)
        {
            return null;
        }

        EnsureIsCurrentPlayer(session, request.PlayerId);

        if (session.AnsweredRoundNumber == session.RoundNumber)
        {
            if (session.PendingRoundResult is null)
            {
                throw new GameRuleException("Esta rodada já foi respondida, mas o resultado não está disponível.");
            }

            return new SubmitAnswerServiceResult
            {
                Result = await ToAnswerResultDtoAsync(session, session.PendingRoundResult),
                StateChanged = false
            };
        }

        if (session.Status != GameStatus.InProgress)
        {
            throw new GameRuleException("A partida não está em andamento.");
        }

        var question = await GetCurrentQuestionEntityAsync(session);
        bool isCorrect = request.SelectedOptionIndex == question.CorrectAnswerIndex;

        var outcome = GameRules.ApplyAnswer(session, isCorrect);

        Reward? reward = null;
        if (isCorrect)
        {
            reward = _rewardSelector.Select(new RewardSelectionContext
            {
                Session = session,
                Actor = outcome.PunishedPlayer,
                Receiver = outcome.CurrentPlayer
            }).Reward;
        }

        session.AnsweredRoundNumber = session.RoundNumber;
        session.PendingRoundResult = new PendingRoundResult
        {
            RoundNumber = session.RoundNumber,
            QuestionId = question.Id,
            SelectedOptionIndex = request.SelectedOptionIndex,
            CorrectAnswerIndex = question.CorrectAnswerIndex,
            IsCorrect = isCorrect,
            CurrentPlayerId = outcome.CurrentPlayer.Id,
            PunishedPlayerId = outcome.PunishedPlayer.Id,
            LostClothing = outcome.LostClothing,
            Reward = reward,
            IsGameOver = outcome.IsGameOver,
            WinnerPlayerId = outcome.Winner?.Id
        };

        await _sessionRepository.UpdateAsync(session);

        // Histórico para análise (append-only; falha não interrompe a jogada).
        // O retorno antecipado do replay idempotente garante que estes registros
        // aconteçam uma única vez por rodada.
        await _activityLog.RecordAnswerAsync(
            session.Id, outcome.CurrentPlayer.Id, question.Id, request.SelectedOptionIndex, isCorrect);

        if (reward is not null)
        {
            await _activityLog.RecordRewardAsync(session.Id, outcome.CurrentPlayer.Id, reward);
        }

        if (outcome.LostClothing is not null)
        {
            await _activityLog.RecordEventAsync(session.Id, outcome.PunishedPlayer.Id, GameEventTypes.ClothingLost, new
            {
                clothing = outcome.LostClothing.ToString(),
                score = outcome.PunishedPlayer.Score,
                roundNumber = session.RoundNumber
            });
        }

        if (outcome.IsGameOver)
        {
            await _activityLog.RecordEventAsync(session.Id, outcome.Winner?.Id, GameEventTypes.GameFinished, new
            {
                winnerName = outcome.Winner?.Name,
                loserScore = outcome.PunishedPlayer.Score,
                roundNumber = session.RoundNumber
            });
        }

        return new SubmitAnswerServiceResult
        {
            Result = await ToAnswerResultDtoAsync(session, session.PendingRoundResult),
            StateChanged = true
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
            if (session.AnsweredRoundNumber != session.RoundNumber)
            {
                throw new GameRuleException("Responda a pergunta antes de avançar a rodada.");
            }

            session.CurrentPlayerIndex = 1 - session.CurrentPlayerIndex;
            session.CurrentQuestionIndex = (session.CurrentQuestionIndex + 1) % session.QuestionOrder.Count;
            session.RoundNumber++;
            session.PendingRoundResult = null;
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

        if (session.Players.Count != 2)
        {
            throw new GameRuleException("A partida precisa de dois jogadores para ser reiniciada.");
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
        session.RoundNumber = 1;
        session.AnsweredRoundNumber = null;
        session.PendingRoundResult = null;
        session.RewardProgression = new RewardProgressionState();
        session.Status = GameStatus.InProgress;
        session.WinnerPlayerId = null;
        session.FinishedAt = null;

        await _sessionRepository.UpdateAsync(session);

        await _activityLog.RecordEventAsync(session.Id, null, GameEventTypes.GameRestarted);

        return await ToStateDtoAsync(session);
    }

    // Encerra a partida para os dois jogadores. Qualquer jogador pode encerrar
    // a qualquer momento (sem validação de vez); a chamada é idempotente.
    public async Task<AbandonGameResultDto?> AbandonAsync(Guid gameId, Guid? playerId = null)
    {
        var session = await _sessionRepository.GetAsync(gameId);
        if (session is null)
        {
            return null;
        }

        var abandonedBy = playerId.HasValue
            ? session.Players.FirstOrDefault(p => p.Id == playerId.Value)
            : null;

        if (session.Status is GameStatus.Finished or GameStatus.Abandoned)
        {
            return new AbandonGameResultDto
            {
                AbandonedByName = abandonedBy?.Name,
                State = await ToStateDtoAsync(session)
            };
        }

        session.Status = GameStatus.Abandoned;
        session.FinishedAt = DateTime.UtcNow;
        session.PendingRoundResult = null;

        await _sessionRepository.UpdateAsync(session);

        await _activityLog.RecordEventAsync(session.Id, abandonedBy?.Id, GameEventTypes.GameAbandoned, new
        {
            playerName = abandonedBy?.Name,
            roundNumber = session.RoundNumber
        });

        return new AbandonGameResultDto
        {
            AbandonedByName = abandonedBy?.Name,
            State = await ToStateDtoAsync(session)
        };
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

    private static RewardDto? ToRewardDto(Reward? reward)
    {
        if (reward?.TemplateId is null || reward.CatalogVersion is null)
        {
            return null;
        }

        return new RewardDto
        {
            TemplateId = reward.TemplateId,
            CatalogVersion = reward.CatalogVersion,
            Text = reward.Text,
            Level = reward.IntensityLevel,
            LevelName = RewardRules.LevelName(reward.IntensityLevel),
            ActorPlayerId = reward.ActorPlayerId,
            ReceiverPlayerId = reward.ReceiverPlayerId,
            ExecutionType = reward.ExecutionType.ToString(),
            ExecutionValue = reward.ExecutionValue
        };
    }

    private static PendingRoundResultDto? ToPendingRoundResultDto(PendingRoundResult? pending)
    {
        if (pending is null)
        {
            return null;
        }

        return new PendingRoundResultDto
        {
            RoundNumber = pending.RoundNumber,
            CorrectAnswerIndex = pending.CorrectAnswerIndex,
            IsCorrect = pending.IsCorrect,
            CurrentPlayerId = pending.CurrentPlayerId,
            PunishedPlayerId = pending.PunishedPlayerId,
            LostClothing = pending.LostClothing?.ToString(),
            Reward = pending.Reward?.Text,
            RewardDetails = ToRewardDto(pending.Reward),
            IsGameOver = pending.IsGameOver,
            WinnerPlayerId = pending.WinnerPlayerId
        };
    }

    private async Task<AnswerResultDto> ToAnswerResultDtoAsync(
        GameSession session,
        PendingRoundResult pending)
    {
        var currentPlayer = session.Players.Single(player => player.Id == pending.CurrentPlayerId);
        var punishedPlayer = session.Players.Single(player => player.Id == pending.PunishedPlayerId);
        var winner = pending.WinnerPlayerId.HasValue
            ? session.Players.SingleOrDefault(player => player.Id == pending.WinnerPlayerId.Value)
            : null;

        return new AnswerResultDto
        {
            IsCorrect = pending.IsCorrect,
            CorrectAnswerIndex = pending.CorrectAnswerIndex,
            CurrentPlayer = ToPlayerDto(currentPlayer),
            PunishedPlayer = ToPlayerDto(punishedPlayer),
            LostClothing = pending.LostClothing?.ToString(),
            Reward = pending.Reward?.Text,
            RewardDetails = ToRewardDto(pending.Reward),
            IsGameOver = pending.IsGameOver,
            Winner = winner is null ? null : ToPlayerDto(winner),
            Message = pending.IsCorrect ? "Correto!" : "Errado!",
            State = await ToStateDtoAsync(session)
        };
    }

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
            RoundNumber = session.RoundNumber,
            Players = session.Players.Select(ToPlayerDto).ToList(),
            CurrentQuestion = session.Status == GameStatus.InProgress
                ? ToQuestionDto(await GetCurrentQuestionEntityAsync(session))
                : null,
            PendingRoundResult = ToPendingRoundResultDto(session.PendingRoundResult),
            WinnerPlayerId = session.WinnerPlayerId,
            WinnerName = winner?.Name,
            CreatedAt = session.CreatedAt,
            FinishedAt = session.FinishedAt
        };
    }
}
