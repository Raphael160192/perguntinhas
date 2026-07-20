using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Infrastructure.Persistence.Configurations;

public class GameSessionEntityConfiguration : IEntityTypeConfiguration<GameSessionEntity>
{
    public void Configure(EntityTypeBuilder<GameSessionEntity> builder)
    {
        builder.ToTable("game_sessions", table =>
        {
            table.HasCheckConstraint("CK_game_sessions_RoundNumber", "\"RoundNumber\" > 0");
            table.HasCheckConstraint("CK_game_sessions_Version", "\"Version\" >= 0");
        });

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status).HasMaxLength(20).IsRequired();
        builder.Property(s => s.Mode).HasMaxLength(20).IsRequired();
        builder.Property(s => s.JoinCode).HasMaxLength(8);
        builder.Property(s => s.AccessChannel)
            .HasMaxLength(20)
            .HasDefaultValue("anonymous")
            .IsRequired();

        // FK opcional para users: a exclusão de conta (D8) anula o UserId sem
        // apagar a partida — os dados de jogo permanecem anonimizados.
        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Property(s => s.QuestionOrderJson).HasColumnType("jsonb").IsRequired();
        builder.Property(s => s.RoundNumber).HasDefaultValue(1).IsRequired();
        builder.Property(s => s.RewardProgressionJson)
            .HasColumnType("jsonb")
            .HasDefaultValue("{\"currentLevel\":1,\"rewardsGeneratedInCurrentStage\":0,\"recentRewards\":[]}")
            .IsRequired();
        builder.Property(s => s.PendingRoundResultJson).HasColumnType("jsonb");
        builder.Property(s => s.Version).IsConcurrencyToken();

        // Busca de sala por código: único apenas entre salas que ainda têm código.
        builder.HasIndex(s => s.JoinCode)
            .IsUnique()
            .HasFilter("\"JoinCode\" IS NOT NULL");

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
