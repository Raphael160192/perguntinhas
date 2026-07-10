using Game.Domain.Entities;

namespace Game.Application.Repositories;

public interface IRewardProvider
{
    Reward GenerateRandomReward();
}
