using Game.Infrastructure.Data;
using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Infrastructure.Persistence.Seed;

// Sincroniza o catálogo de perguntas sem alterar registros já persistidos.
// O texto é a chave natural do seed: execuções posteriores inserem apenas itens ausentes.
public static class QuestionSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext)
    {
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
}
