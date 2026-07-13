using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Domain.Rules;

namespace Game.Application.Rewards;

public class RewardSelector : IRewardSelector
{
    private readonly IRewardCatalog _catalog;
    private readonly IRandomSource _random;

    public RewardSelector(IRewardCatalog catalog, IRandomSource random)
    {
        _catalog = catalog;
        _random = random;
    }

    public RewardSelectionResult Select(RewardSelectionContext context)
    {
        var progression = context.Session.RewardProgression;
        int currentLevel = RewardRules.CalculateLevel(context.Session);

        if (progression.CurrentLevel != currentLevel)
        {
            progression.CurrentLevel = currentLevel;
            progression.RewardsGeneratedInCurrentStage = 0;
        }

        int targetLevel = ChooseTargetLevel(currentLevel, progression.RewardsGeneratedInCurrentStage);
        int alternateLevel = targetLevel == currentLevel ? Math.Max(1, currentLevel - 1) : currentLevel;

        var attempts = new[]
        {
            new SelectionAttempt(targetLevel, RelaxCooldown: false, IsFallback: false),
            new SelectionAttempt(alternateLevel, RelaxCooldown: false, IsFallback: true),
            new SelectionAttempt(targetLevel, RelaxCooldown: true, IsFallback: true),
            new SelectionAttempt(alternateLevel, RelaxCooldown: true, IsFallback: true)
        }.Distinct().ToList();

        foreach (var attempt in attempts)
        {
            var candidates = GetCandidates(context, attempt.Level, attempt.RelaxCooldown);
            if (candidates.Count == 0)
            {
                continue;
            }

            var template = ChooseWeightedTemplate(candidates, progression.RecentRewards);
            string executionValue = ChooseExecutionValue(template, progression.RecentRewards);
            var reward = Instantiate(template, executionValue, context);

            progression.RewardsGeneratedInCurrentStage++;
            progression.RecentRewards.Add(new RecentRewardSnapshot
            {
                TemplateId = template.Id,
                ActionFamily = template.ActionFamily,
                Location = template.Location,
                ExecutionValue = executionValue,
                IntensityLevel = (int)template.IntensityLevel,
                RoundNumber = context.Session.RoundNumber
            });

            if (progression.RecentRewards.Count > RewardRules.MaxRecentRewards)
            {
                progression.RecentRewards = progression.RecentRewards
                    .TakeLast(RewardRules.MaxRecentRewards)
                    .ToList();
            }

            return new RewardSelectionResult
            {
                Reward = reward,
                CurrentLevel = currentLevel,
                TargetLevel = targetLevel,
                CandidateCount = candidates.Count,
                UsedFallback = attempt.IsFallback,
                RelaxedCooldown = attempt.RelaxCooldown
            };
        }

        return new RewardSelectionResult
        {
            CurrentLevel = currentLevel,
            TargetLevel = targetLevel,
            FailureReason = "Nenhum template de prêmio elegível para o contexto atual."
        };
    }

    private int ChooseTargetLevel(int currentLevel, int generatedInStage)
    {
        if (currentLevel == 1)
        {
            return 1;
        }

        double currentLevelProbability = generatedInStage < 2 ? 0.5 : 0.75;
        return _random.NextDouble() < currentLevelProbability ? currentLevel : currentLevel - 1;
    }

    private List<RewardTemplate> GetCandidates(
        RewardSelectionContext context,
        int level,
        bool relaxCooldown)
    {
        var recent = context.Session.RewardProgression.RecentRewards;
        var candidates = _catalog.Templates
            .Where(template => template.Active)
            .Where(template => (int)template.IntensityLevel == level)
            .Where(template => template.ActorRole == RewardActorRole.Opponent)
            .Where(template => template.ReceiverRole == RewardReceiverRole.Winner)
            .Where(template => RewardRules.ClothingRequirementsSatisfied(template, context.Receiver))
            .Where(template => relaxCooldown || !IsInCooldown(template, context.Session.RoundNumber, recent))
            .ToList();

        string? lastFamily = recent.LastOrDefault()?.ActionFamily;
        if (!string.IsNullOrEmpty(lastFamily) &&
            candidates.Any(template => !template.ActionFamily.Equals(lastFamily, StringComparison.OrdinalIgnoreCase)))
        {
            candidates = candidates
                .Where(template => !template.ActionFamily.Equals(lastFamily, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return candidates;
    }

    private static bool IsInCooldown(
        RewardTemplate template,
        int currentRound,
        IReadOnlyCollection<RecentRewardSnapshot> recent)
    {
        return recent.Any(item =>
            item.TemplateId.Equals(template.Id, StringComparison.OrdinalIgnoreCase) &&
            currentRound - item.RoundNumber <= template.CooldownRounds);
    }

    private RewardTemplate ChooseWeightedTemplate(
        IReadOnlyList<RewardTemplate> candidates,
        IReadOnlyCollection<RecentRewardSnapshot> recent)
    {
        var weighted = candidates
            .Select(template => new
            {
                Template = template,
                Weight = CalculateWeight(template, recent)
            })
            .ToList();

        double totalWeight = weighted.Sum(item => item.Weight);
        if (totalWeight <= 0)
        {
            return candidates[_random.Next(candidates.Count)];
        }

        double cursor = _random.NextDouble() * totalWeight;
        foreach (var item in weighted)
        {
            cursor -= item.Weight;
            if (cursor <= 0)
            {
                return item.Template;
            }
        }

        return weighted[^1].Template;
    }

    private static double CalculateWeight(
        RewardTemplate template,
        IReadOnlyCollection<RecentRewardSnapshot> recent)
    {
        double locationMultiplier = recent.TakeLast(3)
            .Any(item => item.Location.Equals(template.Location, StringComparison.OrdinalIgnoreCase))
            ? 0.5
            : 1.0;

        bool hasNovelExecution = template.AllowedExecutionValues.Any(value =>
            recent.All(item => !item.ExecutionValue.Equals(value, StringComparison.OrdinalIgnoreCase)));
        double executionMultiplier = hasNovelExecution ? 1.25 : 1.0;

        return (double)template.BaseWeight * locationMultiplier * executionMultiplier;
    }

    private string ChooseExecutionValue(
        RewardTemplate template,
        IReadOnlyCollection<RecentRewardSnapshot> recent)
    {
        var novelValues = template.AllowedExecutionValues
            .Where(value => recent.All(item =>
                !item.ExecutionValue.Equals(value, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        var pool = novelValues.Count > 0 ? novelValues : template.AllowedExecutionValues;
        return pool[_random.Next(pool.Count)];
    }

    private static Reward Instantiate(
        RewardTemplate template,
        string executionValue,
        RewardSelectionContext context)
    {
        string text = template.TextTemplate
            .Replace("{actor}", context.Actor.Name, StringComparison.Ordinal)
            .Replace("{receiver}", context.Receiver.Name, StringComparison.Ordinal)
            .Replace("{value}", executionValue, StringComparison.Ordinal);

        int timeInSeconds = template.ExecutionType == RewardExecutionType.Seconds &&
            int.TryParse(executionValue, out int parsedSeconds)
            ? parsedSeconds
            : 0;

        return new Reward
        {
            TemplateId = template.Id,
            CatalogVersion = template.CatalogVersion,
            RenderedText = text,
            Action = template.ActionFamily,
            Location = template.Location,
            TimeInSeconds = timeInSeconds,
            IntensityLevel = (int)template.IntensityLevel,
            ExecutionType = template.ExecutionType,
            ExecutionValue = executionValue,
            ActorPlayerId = context.Actor.Id,
            ReceiverPlayerId = context.Receiver.Id,
            RoundNumber = context.Session.RoundNumber,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private sealed record SelectionAttempt(int Level, bool RelaxCooldown, bool IsFallback);
}
