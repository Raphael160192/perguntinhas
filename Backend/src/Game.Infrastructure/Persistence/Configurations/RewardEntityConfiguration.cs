using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Infrastructure.Persistence.Configurations;

public class RewardEntityConfiguration : IEntityTypeConfiguration<RewardEntity>
{
    public void Configure(EntityTypeBuilder<RewardEntity> builder)
    {
        builder.ToTable("rewards");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Text).IsRequired();
        builder.Property(r => r.Action).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Location).HasMaxLength(50).IsRequired();
    }
}
