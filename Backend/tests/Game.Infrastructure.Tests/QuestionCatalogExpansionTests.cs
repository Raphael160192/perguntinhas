using Game.Infrastructure.Data;
using Game.Infrastructure.Persistence;
using Game.Infrastructure.Persistence.Entities;
using Game.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Game.Infrastructure.Tests;

public class QuestionCatalogExpansionTests
{
    private static readonly string[] ExpansionThemes = ["Biologia", "Artes", "Cinema"];

    private static IReadOnlyList<Game.Domain.Entities.Question> ExpansionQuestions =>
        QuestionSeedData.GetQuestions()
            .Where(question => ExpansionThemes.Contains(question.Theme, StringComparer.Ordinal))
            .ToList();

    [Fact]
    public void Expansion_has_expected_theme_and_level_distribution()
    {
        var questions = ExpansionQuestions;

        Assert.Equal(60, questions.Count);

        foreach (var theme in ExpansionThemes)
        {
            var themeQuestions = questions.Where(question => question.Theme == theme).ToList();
            Assert.Equal(20, themeQuestions.Count);

            foreach (var level in Enumerable.Range(1, 4))
            {
                Assert.Equal(5, themeQuestions.Count(question => question.Level == level));
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

        foreach (var answerIndex in Enumerable.Range(0, 4))
        {
            Assert.Equal(15, questions.Count(question => question.CorrectAnswerIndex == answerIndex));
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
        dbContext.Questions.Add(existingQuestion);
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

        var expansionTexts = ExpansionQuestions.Select(question => question.Text).ToHashSet(StringComparer.Ordinal);
        var persistedExpansion = persistedQuestions
            .Where(question => expansionTexts.Contains(question.Text))
            .ToList();

        Assert.Equal(60, persistedExpansion.Count);
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
