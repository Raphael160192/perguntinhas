using Game.Application.Repositories;

namespace Game.Application.Rewards;

public class LegacyRewardSelector : IRewardSelector
{
    private readonly IRewardProvider _provider;

    public LegacyRewardSelector(IRewardProvider provider)
    {
        _provider = provider;
    }

    public RewardSelectionResult Select(RewardSelectionContext context)
    {
        var reward = _provider.GenerateRandomReward();
        reward.ActorPlayerId = context.Actor.Id;
        reward.ReceiverPlayerId = context.Receiver.Id;
        reward.RoundNumber = context.Session.RoundNumber;
        reward.GeneratedAt = DateTime.UtcNow;

        return new RewardSelectionResult
        {
            Reward = reward,
            CurrentLevel = context.Session.RewardProgression.CurrentLevel,
            TargetLevel = context.Session.RewardProgression.CurrentLevel,
            CandidateCount = 1
        };
    }
}
