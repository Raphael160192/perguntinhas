using Game.Application.Repositories;
using Game.Domain.Entities;
using Game.Infrastructure.Persistence;
using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Infrastructure.Repositories;

public class PostgresQuestionRepository : IQuestionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PostgresQuestionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Question>> GetAllAsync()
    {
        var entities = await _dbContext.Questions
            .AsNoTracking()
            .Where(q => q.Active)
            .Include(q => q.Options)
            .OrderBy(q => q.Id)
            .ToListAsync();

        return entities.Select(ToDomain).ToList();
    }

    public async Task<Question?> GetByIdAsync(int id)
    {
        var entity = await _dbContext.Questions
            .AsNoTracking()
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id && q.Active);

        return entity is null ? null : ToDomain(entity);
    }

    private static Question ToDomain(QuestionEntity entity) => new()
    {
        Id = entity.Id,
        Text = entity.Text,
        Theme = entity.Theme,
        Level = entity.Level,
        Options = entity.Options
            .OrderBy(o => o.OptionIndex)
            .Select(o => o.Text)
            .ToList(),
        CorrectAnswerIndex = entity.Options
            .Where(o => o.IsCorrect)
            .Select(o => o.OptionIndex)
            .Single()
    };
}
