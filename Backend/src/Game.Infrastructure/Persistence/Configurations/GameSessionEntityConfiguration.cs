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
        builder.Property(s => s.Mode).HasMaxLength(20).IsRequired();
        builder.Property(s => s.JoinCode).HasMaxLength(8);
        builder.Property(s => s.QuestionOrderJson).HasColumnType("jsonb").IsRequired();

        // Busca de sala por código: único apenas entre salas que ainda têm código.
        builder.HasIndex(s => s.JoinCode)
            .IsUnique()
            .HasFilter("\"JoinCode\" IS NOT NULL");

        // Concorrência otimista: usa a coluna de sistema xmin do PostgreSQL.
        // Protege contra ações simultâneas dos dois aparelhos no modo remoto.
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

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
