using Game.Application.Repositories;
using Game.Domain.Entities;
using Game.Infrastructure.Data;

namespace Game.Infrastructure.Repositories;

public class RandomRewardProvider : IRewardProvider
{
    public Reward GenerateRandomReward()
    {
        var random = Random.Shared;

        return new Reward
        {
            Action = PrizeSeedData.Actions[random.Next(PrizeSeedData.Actions.Length)],
            Location = PrizeSeedData.Locations[random.Next(PrizeSeedData.Locations.Length)],
            TimeInSeconds = PrizeSeedData.Times[random.Next(PrizeSeedData.Times.Length)]
        };
    }
}
