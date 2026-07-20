using Game.Application.Repositories;
using Game.Domain.Entities;
using Game.Infrastructure.Persistence;
using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Infrastructure.Repositories;

public class PostgresUserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PostgresUserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> FindByIdAsync(Guid id)
    {
        var entity = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var normalized = email.Trim();
        var entity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == normalized);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<User?> FindByGoogleSubjectAsync(string googleSubject)
    {
        if (string.IsNullOrWhiteSpace(googleSubject))
        {
            return null;
        }

        var entity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.GoogleSubject == googleSubject);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<User> AddAsync(User user)
    {
        var entity = new UserEntity
        {
            Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id,
            GoogleSubject = user.GoogleSubject,
            Email = user.Email,
            AuthProvider = user.AuthProvider,
            CreatedAt = user.CreatedAt == default ? DateTime.UtcNow : user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };

        _dbContext.Users.Add(entity);
        await _dbContext.SaveChangesAsync();

        return ToDomain(entity);
    }

    public async Task UpdateAsync(User user)
    {
        var entity = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (entity is null)
        {
            return;
        }

        entity.GoogleSubject = user.GoogleSubject;
        entity.Email = user.Email;
        entity.AuthProvider = user.AuthProvider;
        entity.LastLoginAt = user.LastLoginAt;

        await _dbContext.SaveChangesAsync();
    }

    private static User ToDomain(UserEntity entity) => new()
    {
        Id = entity.Id,
        GoogleSubject = entity.GoogleSubject,
        Email = entity.Email,
        AuthProvider = entity.AuthProvider,
        CreatedAt = entity.CreatedAt,
        LastLoginAt = entity.LastLoginAt
    };
}
