using Game.Application.Rewards;

namespace Game.Infrastructure.Rewards;

public class SystemRandomSource : IRandomSource
{
    public int Next(int exclusiveMax) => Random.Shared.Next(exclusiveMax);

    public double NextDouble() => Random.Shared.NextDouble();
}
