using Game.Domain.Entities;

namespace Game.Application.Repositories;

// Contrato publicado pela fatia-base (F0) para os fluxos de conta (US6/US7/US9/US13).
public interface IUserRepository
{
    Task<User?> FindByIdAsync(Guid id);
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByGoogleSubjectAsync(string googleSubject);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
}
