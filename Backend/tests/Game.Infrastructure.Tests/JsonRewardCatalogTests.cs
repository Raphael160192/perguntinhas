using Game.Domain.Enums;
using Game.Infrastructure.Rewards;
using Xunit;

namespace Game.Infrastructure.Tests;

public class JsonRewardCatalogTests
{
    [Fact]
    public void Catalog_LoadsAndHasRequiredCoverage()
    {
        var catalog = new JsonRewardCatalog("reward-templates.v1.json");

        Assert.Equal("1.0.0", catalog.Version);
        Assert.Equal(catalog.Templates.Count, catalog.Templates.Select(template => template.Id).Distinct().Count());
        Assert.All(Enumerable.Range(1, 4), level =>
            Assert.True(catalog.Templates.Count(template =>
                template.Active && (int)template.IntensityLevel == level) >= 12));
    }

    [Fact]
    public void Catalog_DoesNotContainForbiddenBiteLocations()
    {
        var catalog = new JsonRewardCatalog("reward-templates.v1.json");
        string[] forbidden = { "Nose", "Face", "Throat" };

        Assert.DoesNotContain(catalog.Templates, template =>
            template.ActionFamily == "GentleBite" && forbidden.Contains(template.Location));
    }

    [Fact]
    public void EveryLevelHasTemplatesForFullyClothedReceiver()
    {
        var catalog = new JsonRewardCatalog("reward-templates.v1.json");

        Assert.All(Enumerable.Range(1, 4), level =>
            Assert.True(catalog.Templates.Count(template =>
                (int)template.IntensityLevel == level &&
                template.RequiredClothingState.Count == 0 &&
                template.Accessibility is RewardAccessibility.Any or
                    RewardAccessibility.OverClothingAllowed) >= 4));
    }
}
