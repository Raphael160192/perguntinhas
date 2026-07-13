namespace Game.Application.Rewards;

public interface IRewardSelector
{
    RewardSelectionResult Select(RewardSelectionContext context);
}
