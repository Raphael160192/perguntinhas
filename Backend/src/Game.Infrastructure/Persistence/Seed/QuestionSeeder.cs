using Game.Infrastructure.Data;
using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Infrastructure.Persistence.Seed;

// Sincroniza o catálogo pelo texto, preservando registros existentes, exceto substituições
// explicitamente catalogadas para perguntas que tiveram sua dificuldade recalibrada.
public static class QuestionSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        await ReplaceSupersededAccessibleQuestionsAsync(dbContext);

        var persistedTexts = await dbContext.Questions
            .AsNoTracking()
            .Select(question => question.Text)
            .ToListAsync();
        var existingTexts = persistedTexts.ToHashSet(StringComparer.Ordinal);

        var questions = QuestionSeedData.GetQuestions()
            .Where(question => !existingTexts.Contains(question.Text))
            .ToList();

        if (questions.Count == 0)
        {
            return;
        }

        var highestId = await dbContext.Questions
            .AsNoTracking()
            .Select(question => (int?)question.Id)
            .MaxAsync() ?? -1;

        if (highestId > int.MaxValue - questions.Count)
        {
            throw new InvalidOperationException("Não há IDs disponíveis para inserir o catálogo de perguntas.");
        }

        var nextId = highestId + 1;
        var now = DateTime.UtcNow;

        var entities = questions.Select(q =>
        {
            var questionEntity = new QuestionEntity
            {
                Id = nextId++,
                Text = q.Text,
                Theme = q.Theme,
                Level = q.Level,
                Active = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            questionEntity.Options = q.Options.Select((optionText, index) => new QuestionOptionEntity
            {
                Id = Guid.NewGuid(),
                QuestionId = questionEntity.Id,
                OptionIndex = index,
                Text = optionText,
                IsCorrect = index == q.CorrectAnswerIndex
            }).ToList();

            return questionEntity;
        }).ToList();

        dbContext.Questions.AddRange(entities);
        await dbContext.SaveChangesAsync();
    }

    private static async Task ReplaceSupersededAccessibleQuestionsAsync(ApplicationDbContext dbContext)
    {
        var replacements = QuestionSeedData.GetAccessibleQuestionReplacements();
        var supersededTexts = replacements.Keys.ToArray();
        var replacementTexts = replacements.Values.Select(question => question.Text).ToArray();

        var supersededQuestions = await dbContext.Questions
            .Include(question => question.Options)
            .Where(question => supersededTexts.Contains(question.Text))
            .ToListAsync();

        if (supersededQuestions.Count == 0)
        {
            return;
        }

        var alreadyPersistedReplacementTexts = await dbContext.Questions
            .AsNoTracking()
            .Where(question => replacementTexts.Contains(question.Text))
            .Select(question => question.Text)
            .ToListAsync();
        var unavailableReplacementTexts = alreadyPersistedReplacementTexts.ToHashSet(StringComparer.Ordinal);
        var now = DateTime.UtcNow;

        foreach (var persistedQuestion in supersededQuestions)
        {
            var replacement = replacements[persistedQuestion.Text];

            if (!unavailableReplacementTexts.Add(replacement.Text))
            {
                persistedQuestion.Active = false;
                persistedQuestion.UpdatedAt = now;
                continue;
            }

            persistedQuestion.Text = replacement.Text;
            persistedQuestion.Theme = replacement.Theme;
            persistedQuestion.Level = replacement.Level;
            persistedQuestion.UpdatedAt = now;

            var optionsByIndex = persistedQuestion.Options.ToDictionary(option => option.OptionIndex);
            for (var index = 0; index < replacement.Options.Count; index++)
            {
                if (!optionsByIndex.TryGetValue(index, out var option))
                {
                    option = new QuestionOptionEntity
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = persistedQuestion.Id,
                        OptionIndex = index
                    };
                    persistedQuestion.Options.Add(option);
                }

                option.Text = replacement.Options[index];
                option.IsCorrect = index == replacement.CorrectAnswerIndex;
            }

            var extraOptions = persistedQuestion.Options
                .Where(option => option.OptionIndex < 0 || option.OptionIndex >= replacement.Options.Count)
                .ToList();
            dbContext.Set<QuestionOptionEntity>().RemoveRange(extraOptions);
        }

        await dbContext.SaveChangesAsync();
    }
}
