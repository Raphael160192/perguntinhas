using Game.Domain.Entities;

namespace Game.Application.Rewards;

public class RewardSelectionContext
{
    public GameSession Session { get; init; } = null!;
    public Player Actor { get; init; } = null!;
    public Player Receiver { get; init; } = null!;
}
