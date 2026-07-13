namespace Game.Application.Rewards;

public interface IRandomSource
{
    int Next(int exclusiveMax);
    double NextDouble();
}
