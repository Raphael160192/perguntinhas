using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Game.Application.Rewards;
using Game.Domain.Entities;
using Game.Domain.Enums;

namespace Game.Infrastructure.Rewards;

public class JsonRewardCatalog : IRewardCatalog
{
    public string Version { get; }
    public IReadOnlyList<RewardTemplate> Templates { get; }

    public JsonRewardCatalog(string resourceFileName)
    {
        var assembly = typeof(JsonRewardCatalog).Assembly;
        string resourceName = assembly.GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith(resourceFileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                $"Catálogo de prêmios embarcado '{resourceFileName}' não encontrado.");

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Não foi possível abrir o catálogo '{resourceName}'.");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());

        var document = JsonSerializer.Deserialize<RewardCatalogDocument>(stream, options)
            ?? throw new InvalidOperationException("O catálogo de prêmios está vazio ou inválido.");

        Version = document.CatalogVersion;
        foreach (var template in document.Templates)
        {
            template.CatalogVersion = Version;
        }

        RewardCatalogValidator.Validate(document);
        Templates = document.Templates.AsReadOnly();
    }

    private sealed class RewardCatalogDocument
    {
        public string CatalogVersion { get; set; } = string.Empty;
        public List<RewardTemplate> Templates { get; set; } = new();
    }

    private static class RewardCatalogValidator
    {
        private static readonly HashSet<string> ForbiddenPairs = new(StringComparer.OrdinalIgnoreCase)
        {
            "GentleBite|Nose",
            "GentleBite|Face",
            "GentleBite|Throat"
        };

        public static void Validate(RewardCatalogDocument document)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(document.CatalogVersion))
            {
                errors.Add("catalogVersion é obrigatório.");
            }

            var duplicateIds = document.Templates
                .Where(template => !string.IsNullOrWhiteSpace(template.Id))
                .GroupBy(template => template.Id, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);
            foreach (string duplicateId in duplicateIds)
            {
                errors.Add($"ID de template duplicado: {duplicateId}.");
            }

            foreach (var template in document.Templates)
            {
                ValidateTemplate(template, errors);
            }

            for (int level = 1; level <= 4; level++)
            {
                var templates = document.Templates
                    .Where(template => template.Active && (int)template.IntensityLevel == level)
                    .ToList();

                if (templates.Count < 12)
                {
                    errors.Add($"Nível {level} precisa ter pelo menos 12 templates ativos.");
                }

                if (templates.Select(template => template.ActionFamily)
                    .Distinct(StringComparer.OrdinalIgnoreCase).Count() < 3)
                {
                    errors.Add($"Nível {level} precisa ter pelo menos 3 famílias de ação.");
                }

                if (templates.Select(template => template.Location)
                    .Distinct(StringComparer.OrdinalIgnoreCase).Count() < 4)
                {
                    errors.Add($"Nível {level} precisa ter pelo menos 4 locais/contextos.");
                }

                int broadlyAccessible = templates.Count(template =>
                    template.RequiredClothingState.Count == 0 &&
                    template.Accessibility is RewardAccessibility.Any or
                        RewardAccessibility.OverClothingAllowed);
                if (broadlyAccessible < 4)
                {
                    errors.Add($"Nível {level} precisa ter pelo menos 4 templates acessíveis com qualquer roupa.");
                }
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Catálogo de prêmios inválido:" + Environment.NewLine +
                    string.Join(Environment.NewLine, errors.Select(error => $"- {error}")));
            }
        }

        private static void ValidateTemplate(RewardTemplate template, List<string> errors)
        {
            string id = string.IsNullOrWhiteSpace(template.Id) ? "<sem-id>" : template.Id;

            if (string.IsNullOrWhiteSpace(template.Id)) errors.Add("Template sem ID.");
            if (string.IsNullOrWhiteSpace(template.TextTemplate)) errors.Add($"{id}: texto obrigatório.");
            if (!template.TextTemplate.Contains("{actor}", StringComparison.Ordinal) ||
                !template.TextTemplate.Contains("{receiver}", StringComparison.Ordinal))
            {
                errors.Add($"{id}: texto precisa conter {{actor}} e {{receiver}}.");
            }

            if (!template.TextTemplate.Contains("{value}", StringComparison.Ordinal))
            {
                errors.Add($"{id}: texto precisa conter {{value}}.");
            }

            if (string.IsNullOrWhiteSpace(template.ActionFamily)) errors.Add($"{id}: família obrigatória.");
            if (string.IsNullOrWhiteSpace(template.Location)) errors.Add($"{id}: local obrigatório.");
            if ((int)template.IntensityLevel is < 1 or > 4) errors.Add($"{id}: nível deve estar entre 1 e 4.");
            if (template.BaseWeight <= 0) errors.Add($"{id}: peso deve ser positivo.");
            if (template.CooldownRounds is < 0 or > 12) errors.Add($"{id}: cooldown deve estar entre 0 e 12.");
            if (template.AllowedExecutionValues.Count == 0) errors.Add($"{id}: valores de execução obrigatórios.");

            if (template.Accessibility == RewardAccessibility.ExposedAreaRequired &&
                template.RequiredClothingState.Count == 0)
            {
                errors.Add($"{id}: acesso exposto exige uma peça removida.");
            }

            if (ForbiddenPairs.Contains($"{template.ActionFamily}|{template.Location}"))
            {
                errors.Add($"{id}: combinação proibida {template.ActionFamily}/{template.Location}.");
            }
        }
    }
}
