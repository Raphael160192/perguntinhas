using Game.Infrastructure.Data;
using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Infrastructure.Persistence.Seed;

// Popula a tabela de perguntas com o mesmo conjunto inicial usado em memória.
// Idempotente: só insere se a tabela ainda estiver vazia.
public static class QuestionSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        bool hasQuestions = await dbContext.Questions.AnyAsync();
        if (hasQuestions)
        {
            return;
        }

        var questions = QuestionSeedData.GetQuestions();
        var now = DateTime.UtcNow;

        var entities = questions.Select(q =>
        {
            var questionEntity = new QuestionEntity
            {
                Id = q.Id,
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
