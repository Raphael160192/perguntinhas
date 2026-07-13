using Game.Domain.Entities;

namespace Game.Application.Rewards;

public class RewardSelectionResult
{
    public Reward? Reward { get; init; }
    public int CurrentLevel { get; init; }
    public int TargetLevel { get; init; }
    public int CandidateCount { get; init; }
    public bool UsedFallback { get; init; }
    public bool RelaxedCooldown { get; init; }
    public string? FailureReason { get; init; }

    public bool Generated => Reward is not null;
}
