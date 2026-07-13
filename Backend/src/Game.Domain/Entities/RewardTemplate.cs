using Game.Domain.Enums;

namespace Game.Domain.Entities;

public class RewardTemplate
{
    public string Id { get; set; } = string.Empty;
    public string CatalogVersion { get; set; } = string.Empty;
    public string TextTemplate { get; set; } = string.Empty;
    public string ActionFamily { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public RewardIntensityLevel IntensityLevel { get; set; }
    public RewardExecutionType ExecutionType { get; set; }
    public List<string> AllowedExecutionValues { get; set; } = new();
    public RewardActorRole ActorRole { get; set; } = RewardActorRole.Opponent;
    public RewardReceiverRole ReceiverRole { get; set; } = RewardReceiverRole.Winner;
    public RewardAccessibility Accessibility { get; set; }
    public List<ClothingItem> RequiredClothingState { get; set; } = new();
    public List<string> ContentTags { get; set; } = new();
    public int CooldownRounds { get; set; }
    public decimal BaseWeight { get; set; } = 1m;
    public bool Active { get; set; } = true;
}
