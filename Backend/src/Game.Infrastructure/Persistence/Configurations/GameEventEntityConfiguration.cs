using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Infrastructure.Persistence.Configurations;

public class GameEventEntityConfiguration : IEntityTypeConfiguration<GameEventEntity>
{
    public void Configure(EntityTypeBuilder<GameEventEntity> builder)
    {
        builder.ToTable("game_events");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityByDefaultColumn();

        builder.Property(e => e.EventType).HasMaxLength(40).IsRequired();
        builder.Property(e => e.PayloadJson).HasColumnType("jsonb").IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.GameSessionId);
        builder.HasIndex(e => e.EventType);

        builder.HasOne(e => e.GameSession)
            .WithMany()
            .HasForeignKey(e => e.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK opcional para users (D8: exclusão anula o vínculo, mantém o evento).
        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
