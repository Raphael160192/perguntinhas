using Game.Domain.Entities;
using Game.Domain.Enums;

namespace Game.Domain.Rules;

public static class RewardRules
{
    public const int MinLevel = 1;
    public const int MaxLevel = 4;
    public const int MaxRecentRewards = 12;

    public static int CalculateLevel(GameSession session)
    {
        int mostLostClothes = session.Players
            .Select(player => 4 - player.Clothes.RemainingCount())
            .DefaultIfEmpty(0)
            .Max();

        return Math.Clamp(1 + mostLostClothes, MinLevel, MaxLevel);
    }

    public static bool ClothingRequirementsSatisfied(RewardTemplate template, Player receiver)
    {
        return template.RequiredClothingState.All(item => !receiver.Clothes.HasItem(item));
    }

    public static string LevelName(int level) => level switch
    {
        1 => "Conexão",
        2 => "Aproximação",
        3 => "Tensão",
        4 => "Intimidade",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Nível de prêmio inválido.")
    };
}
