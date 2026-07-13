namespace Game.Domain.Enums;

public enum RewardIntensityLevel
{
    Connection = 1,
    Approach = 2,
    Tension = 3,
    Intimacy = 4
}

public enum RewardExecutionType
{
    Seconds,
    Repetitions,
    FreeForm
}

public enum RewardAccessibility
{
    Any,
    OverClothingAllowed,
    ExposedAreaRequired
}

public enum RewardActorRole
{
    Opponent
}

public enum RewardReceiverRole
{
    Winner
}
