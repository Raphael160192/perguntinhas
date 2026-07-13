namespace Game.Domain.Entities;

public class RewardProgressionState
{
    public int CurrentLevel { get; set; } = 1;
    public int RewardsGeneratedInCurrentStage { get; set; }
    public List<RecentRewardSnapshot> RecentRewards { get; set; } = new();
}

public class RecentRewardSnapshot
{
    public string TemplateId { get; set; } = string.Empty;
    public string ActionFamily { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ExecutionValue { get; set; } = string.Empty;
    public int IntensityLevel { get; set; }
    public int RoundNumber { get; set; }
}
