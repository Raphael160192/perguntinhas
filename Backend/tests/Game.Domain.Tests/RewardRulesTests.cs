using Game.Domain.Entities;
using Game.Domain.Enums;
using Game.Domain.Rules;
using Xunit;

namespace Game.Domain.Tests;

public class RewardRulesTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    [InlineData(3, 4)]
    [InlineData(4, 4)]
    public void CalculateLevel_UsesMostExposedPlayer(int lostClothes, int expectedLevel)
    {
        var session = CreateSession();
        foreach (var item in GameRules.ClothingOrder.Take(lostClothes))
        {
            session.Players[1].Clothes.LoseItem(item);
        }

        Assert.Equal(expectedLevel, RewardRules.CalculateLevel(session));
    }

    [Fact]
    public void ClothingRequirementsSatisfied_UsesReceiverState()
    {
        var player = new Player();
        var template = new RewardTemplate
        {
            RequiredClothingState = new List<ClothingItem> { ClothingItem.Shirt }
        };

        Assert.False(RewardRules.ClothingRequirementsSatisfied(template, player));

        player.Clothes.LoseItem(ClothingItem.Shirt);

        Assert.True(RewardRules.ClothingRequirementsSatisfied(template, player));
    }

    private static GameSession CreateSession() => new()
    {
        Players = new List<Player> { new(), new() }
    };
}
