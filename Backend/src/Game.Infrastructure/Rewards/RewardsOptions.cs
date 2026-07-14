namespace Game.Infrastructure.Rewards;

public class RewardsOptions
{
    public const string SectionName = "Rewards";

    public bool IntelligentSelectionEnabled { get; set; } = true;
    public string CatalogResource { get; set; } = "reward-templates.v1.json";
}
