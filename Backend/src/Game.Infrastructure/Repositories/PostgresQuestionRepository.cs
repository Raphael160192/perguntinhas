using Game.Application.Repositories;
using Game.Domain.Entities;
using Game.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Game.Infrastructure.Repositories;

public class PostgresQuestionRepository : IQuestionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PostgresQuestionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<Question> GetAll()
    {
        return _dbContext.Questions
            .AsNoTracking()
            .Where(q => q.Active)
            .Include(q => q.Options)
            .OrderBy(q => q.Id)
            .Select(q => new Question
            {
                Id = q.Id,
                Text = q.Text,
                Theme = q.Theme,
                Level = q.Level,
                Options = q.Options
                    .OrderBy(o => o.OptionIndex)
                    .Select(o => o.Text)
                    .ToList(),
                CorrectAnswerIndex = q.Options
                    .Where(o => o.IsCorrect)
                    .Select(o => o.OptionIndex)
                    .Single()
            })
            .ToList();
    }
}
