using Game.Domain.Entities;

namespace Game.Application.Rewards;

public interface IRewardCatalog
{
    string Version { get; }
    IReadOnlyList<RewardTemplate> Templates { get; }
}
