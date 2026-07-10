using Game.Domain.Entities;

namespace Game.Application.Repositories;

public interface IQuestionRepository
{
    List<Question> GetAll();
}
