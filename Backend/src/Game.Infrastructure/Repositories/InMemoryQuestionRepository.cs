using Game.Application.Repositories;
using Game.Domain.Entities;
using Game.Infrastructure.Data;

namespace Game.Infrastructure.Repositories;

public class InMemoryQuestionRepository : IQuestionRepository
{
    private readonly List<Question> _questions = QuestionSeedData.GetQuestions();

    public List<Question> GetAll() => _questions;
}
