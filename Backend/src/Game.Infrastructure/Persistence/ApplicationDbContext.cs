using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<GameSessionEntity> GameSessions => Set<GameSessionEntity>();
    public DbSet<GamePlayerEntity> GamePlayers => Set<GamePlayerEntity>();
    public DbSet<QuestionEntity> Questions => Set<QuestionEntity>();
    public DbSet<QuestionOptionEntity> QuestionOptions => Set<QuestionOptionEntity>();
    public DbSet<GameAnswerEntity> GameAnswers => Set<GameAnswerEntity>();
    public DbSet<RewardEntity> Rewards => Set<RewardEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
