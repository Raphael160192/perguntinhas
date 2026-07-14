using Game.Infrastructure.Data;
using Game.Infrastructure.Persistence;
using Game.Infrastructure.Persistence.Entities;
using Game.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Game.Infrastructure.Tests;

public class QuestionCatalogExpansionTests
{
    private static readonly string[] AllThemes =
    [
        "Harry Potter",
        "Naruto",
        "História do Brasil",
        "Geografia",
        "Atualidades",
        "Filmes da Disney",
        "Biologia",
        "Artes",
        "Cinema"
    ];

    private static readonly string[] ExpansionThemes = ["Biologia", "Artes", "Cinema"];

    private static IReadOnlyList<Game.Domain.Entities.Question> ExpansionQuestions =>
        QuestionSeedData.GetQuestions()
            .Where(question => ExpansionThemes.Contains(question.Theme, StringComparer.Ordinal))
            .ToList();

    [Fact]
    public void Catalog_has_five_more_accessible_questions_in_every_theme()
    {
        var questions = QuestionSeedData.GetQuestions();

        Assert.Equal(165, questions.Count);

        foreach (var theme in AllThemes)
        {
            var themeQuestions = questions.Where(question => question.Theme == theme).ToList();
            var isExpansionTheme = ExpansionThemes.Contains(theme, StringComparer.Ordinal);

            Assert.Equal(isExpansionTheme ? 25 : 15, themeQuestions.Count);
            Assert.Equal(isExpansionTheme ? 10 : 15, themeQuestions.Count(question => question.Level == 1));

            foreach (var level in Enumerable.Range(2, 3))
            {
                Assert.Equal(isExpansionTheme ? 5 : 0, themeQuestions.Count(question => question.Level == level));
            }
        }
    }

    [Fact]
    public void Expansion_has_valid_options_and_balanced_correct_answers()
    {
        var questions = ExpansionQuestions;

        Assert.All(questions, question =>
        {
            Assert.Equal(4, question.Options.Count);
            Assert.Equal(4, question.Options.Distinct(StringComparer.Ordinal).Count());
            Assert.InRange(question.CorrectAnswerIndex, 0, 3);
            Assert.Contains(question.Theme, ExpansionThemes);
            Assert.InRange(question.Level, 1, 4);
        });

        var expectedAnswerCounts = new[] {19, 18, 19, 19};
        foreach (var answerIndex in Enumerable.Range(0, expectedAnswerCounts.Length))
        {
            Assert.Equal(
                expectedAnswerCounts[answerIndex],
                questions.Count(question => question.CorrectAnswerIndex == answerIndex));
        }
    }

    [Fact]
    public void Difficulty_replacements_cover_all_new_questions_without_leaving_superseded_texts()
    {
        var replacements = QuestionSeedData.GetAccessibleQuestionReplacements();
        var catalog = QuestionSeedData.GetQuestions();
        var catalogTexts = catalog.Select(question => question.Text).ToHashSet(StringComparer.Ordinal);

        Assert.Equal(45, replacements.Count);
        Assert.All(replacements.Keys, text => Assert.DoesNotContain(text, catalogTexts));
        Assert.Equal(45, replacements.Values.Select(question => question.Text).Distinct(StringComparer.Ordinal).Count());

        foreach (var theme in AllThemes)
        {
            Assert.Equal(5, replacements.Values.Count(question => question.Theme == theme));
        }
    }

    [Fact]
    public void Complete_catalog_has_no_duplicate_question_texts()
    {
        var questions = QuestionSeedData.GetQuestions();

        Assert.Equal(
            questions.Count,
            questions.Select(question => question.Text).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task Seeder_is_idempotent_and_adds_catalog_to_populated_database()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationDbContext(options);
        var existingQuestion = new QuestionEntity
        {
            Id = 500,
            Text = "Pergunta que já existia no banco",
            Theme = "Tema legado",
            Level = 1,
            Active = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        const string supersededText = "Qual é o formato da cicatriz de Harry Potter?";
        var replacement = QuestionSeedData.GetAccessibleQuestionReplacements()[supersededText];
        var supersededQuestion = new QuestionEntity
        {
            Id = 501,
            Text = supersededText,
            Theme = "Harry Potter",
            Level = 1,
            Active = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Options = Enumerable.Range(0, 4).Select(index => new QuestionOptionEntity
            {
                Id = Guid.NewGuid(),
                QuestionId = 501,
                OptionIndex = index,
                Text = $"Alternativa antiga {index}",
                IsCorrect = index == 0
            }).ToList()
        };
        dbContext.Questions.AddRange(existingQuestion, supersededQuestion);
        await dbContext.SaveChangesAsync();

        await QuestionSeeder.SeedAsync(dbContext);
        await QuestionSeeder.SeedAsync(dbContext);

        var persistedQuestions = await dbContext.Questions
            .AsNoTracking()
            .Include(question => question.Options)
            .ToListAsync();

        Assert.Equal(QuestionSeedData.GetQuestions().Count + 1, persistedQuestions.Count);
        Assert.Contains(persistedQuestions, question =>
            question.Id == existingQuestion.Id &&
            question.Text == existingQuestion.Text &&
            !question.Active);
        Assert.DoesNotContain(persistedQuestions, question => question.Text == supersededText);
        Assert.Contains(persistedQuestions, question =>
            question.Id == supersededQuestion.Id &&
            question.Text == replacement.Text &&
            question.Options.OrderBy(option => option.OptionIndex)
                .Select(option => option.Text)
                .SequenceEqual(replacement.Options) &&
            question.Options.Single(option => option.IsCorrect).OptionIndex == replacement.CorrectAnswerIndex);

        var expansionTexts = ExpansionQuestions.Select(question => question.Text).ToHashSet(StringComparer.Ordinal);
        var persistedExpansion = persistedQuestions
            .Where(question => expansionTexts.Contains(question.Text))
            .ToList();

        Assert.Equal(75, persistedExpansion.Count);
        Assert.All(persistedExpansion, question =>
        {
            Assert.True(question.Active);
            Assert.Equal(4, question.Options.Count);
            Assert.Single(question.Options, option => option.IsCorrect);
        });
        Assert.Equal(persistedQuestions.Count, persistedQuestions.Select(question => question.Id).Distinct().Count());
        Assert.Equal(persistedQuestions.Count, persistedQuestions.Select(question => question.Text).Distinct(StringComparer.Ordinal).Count());
    }
}
