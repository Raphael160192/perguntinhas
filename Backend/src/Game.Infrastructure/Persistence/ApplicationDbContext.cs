using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<QuestionEntity> Questions => Set<QuestionEntity>();
    public DbSet<QuestionOptionEntity> QuestionOptions => Set<QuestionOptionEntity>();
    public DbSet<GameSessionEntity> GameSessions => Set<GameSessionEntity>();
    public DbSet<GamePlayerEntity> GamePlayers => Set<GamePlayerEntity>();
    public DbSet<GameAnswerEntity> GameAnswers => Set<GameAnswerEntity>();
    public DbSet<RewardEntity> Rewards => Set<RewardEntity>();
    public DbSet<GameEventEntity> GameEvents => Set<GameEventEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<UserConsentEntity> UserConsents => Set<UserConsentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
