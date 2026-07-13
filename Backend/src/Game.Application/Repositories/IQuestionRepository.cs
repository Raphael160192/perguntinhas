using Game.Domain.Entities;

namespace Game.Application.Repositories;

public interface IQuestionRepository
{
    Task<List<Question>> GetAllAsync();
    Task<Question?> GetByIdAsync(int id);
}
