using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Infrastructure.Persistence.Configurations;

public class GameAnswerEntityConfiguration : IEntityTypeConfiguration<GameAnswerEntity>
{
    public void Configure(EntityTypeBuilder<GameAnswerEntity> builder)
    {
        builder.ToTable("game_answers");

        builder.HasKey(a => a.Id);
    }
}
