using Game.Application.Rewards;
using Game.Domain.Entities;
using Game.Domain.Enums;
using Xunit;

namespace Game.Application.Tests;

public class RewardSelectorTests
{
    [Theory]
    [InlineData(0, 0.49, 2)]
    [InlineData(0, 0.51, 1)]
    [InlineData(2, 0.74, 2)]
    [InlineData(2, 0.76, 1)]
    public void Select_UsesConfiguredStageDistribution(
        int generatedInStage,
        double randomValue,
        int expectedLevel)
    {
        var session = CreateLevelTwoSession(generatedInStage);
        var selector = CreateSelector(AllLevelsCatalog(), randomValue);

        var result = selector.Select(Context(session));

        Assert.NotNull(result.Reward);
        Assert.Equal(expectedLevel, result.Reward!.IntensityLevel);
    }

    [Fact]
    public void Select_FiltersTemplateByReceiversClothing()
    {
        var session = CreateLevelTwoSession(0);
        var exposed = Template("exposed", 2, required: ClothingItem.Shirt);
        var broadlyAccessible = Template("accessible", 2);
        var selector = CreateSelector(new[] { exposed, broadlyAccessible, Template("lower", 1) }, 0.1);

        var result = selector.Select(Context(session));

        Assert.Equal("accessible", result.Reward?.TemplateId);
    }

    [Fact]
    public void Select_UsesAlternativeWhenTemplateIsInCooldown()
    {
        var session = CreateLevelTwoSession(0);
        session.RoundNumber = 5;
        session.RewardProgression.RecentRewards.Add(new RecentRewardSnapshot
        {
            TemplateId = "cooling",
            ActionFamily = "Kiss",
            Location = "Mouth",
            ExecutionValue = "10",
            IntensityLevel = 2,
            RoundNumber = 4
        });
        var selector = CreateSelector(new[]
        {
            Template("cooling", 2, family: "Kiss", cooldown: 4),
            Template("alternative", 2, family: "Massage"),
            Template("lower", 1)
        }, 0.1);

        var result = selector.Select(Context(session));

        Assert.Equal("alternative", result.Reward?.TemplateId);
        Assert.False(result.RelaxedCooldown);
    }

    [Fact]
    public void Select_ReturnsNoRewardWithoutMutatingCounterWhenNoCandidateExists()
    {
        var session = CreateLevelTwoSession(1);
        var selector = CreateSelector(Array.Empty<RewardTemplate>(), 0.1);

        var result = selector.Select(Context(session));

        Assert.False(result.Generated);
        Assert.Equal(1, session.RewardProgression.RewardsGeneratedInCurrentStage);
        Assert.Empty(session.RewardProgression.RecentRewards);
    }

    private static RewardSelector CreateSelector(IEnumerable<RewardTemplate> templates, double value) =>
        new(new FakeCatalog(templates), new FakeRandomSource(value));

    private static RewardSelectionContext Context(GameSession session) => new()
    {
        Session = session,
        Actor = session.Players[1],
        Receiver = session.Players[0]
    };

    private static GameSession CreateLevelTwoSession(int generatedInStage)
    {
        var session = new GameSession
        {
            Players = new List<Player>
            {
                new() { Name = "A" },
                new() { Name = "B" }
            },
            RewardProgression = new RewardProgressionState
            {
                CurrentLevel = 2,
                RewardsGeneratedInCurrentStage = generatedInStage
            }
        };
        session.Players[1].Clothes.LoseItem(ClothingItem.Socks);
        return session;
    }

    private static IReadOnlyList<RewardTemplate> AllLevelsCatalog() => new[]
    {
        Template("level-one", 1),
        Template("level-two", 2)
    };

    private static RewardTemplate Template(
        string id,
        int level,
        string family = "Massage",
        int cooldown = 4,
        ClothingItem? required = null) =>
        new()
        {
            Id = id,
            CatalogVersion = "test",
            TextTemplate = "{actor} faz algo com {receiver} por {value}",
            ActionFamily = family,
            Location = id,
            IntensityLevel = (RewardIntensityLevel)level,
            ExecutionType = RewardExecutionType.Seconds,
            AllowedExecutionValues = new List<string> { "10" },
            Accessibility = required.HasValue
                ? RewardAccessibility.ExposedAreaRequired
                : RewardAccessibility.Any,
            RequiredClothingState = required.HasValue
                ? new List<ClothingItem> { required.Value }
                : new List<ClothingItem>(),
            CooldownRounds = cooldown,
            BaseWeight = 1,
            Active = true
        };

    private sealed class FakeCatalog : IRewardCatalog
    {
        public FakeCatalog(IEnumerable<RewardTemplate> templates)
        {
            Templates = templates.ToList();
        }

        public string Version => "test";
        public IReadOnlyList<RewardTemplate> Templates { get; }
    }

    private sealed class FakeRandomSource : IRandomSource
    {
        private readonly double _value;

        public FakeRandomSource(double value)
        {
            _value = value;
        }

        public int Next(int exclusiveMax) => 0;
        public double NextDouble() => _value;
    }
}
