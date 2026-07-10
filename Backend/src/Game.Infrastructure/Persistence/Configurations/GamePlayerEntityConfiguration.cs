using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Infrastructure.Persistence.Configurations;

public class GamePlayerEntityConfiguration : IEntityTypeConfiguration<GamePlayerEntity>
{
    public void Configure(EntityTypeBuilder<GamePlayerEntity> builder)
    {
        builder.ToTable("game_players");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.ClothingLostAtScoresJson).HasColumnType("jsonb").IsRequired();
    }
}
