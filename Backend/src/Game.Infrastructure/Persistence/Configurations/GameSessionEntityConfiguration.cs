using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Infrastructure.Persistence.Configurations;

public class GameSessionEntityConfiguration : IEntityTypeConfiguration<GameSessionEntity>
{
    public void Configure(EntityTypeBuilder<GameSessionEntity> builder)
    {
        builder.ToTable("game_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status).HasMaxLength(20).IsRequired();

        builder.HasMany(s => s.Players)
            .WithOne(p => p.GameSession)
            .HasForeignKey(p => p.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Answers)
            .WithOne(a => a.GameSession)
            .HasForeignKey(a => a.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
