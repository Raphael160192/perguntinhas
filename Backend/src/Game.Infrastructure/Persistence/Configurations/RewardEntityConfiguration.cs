using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Infrastructure.Persistence.Configurations;

public class RewardEntityConfiguration : IEntityTypeConfiguration<RewardEntity>
{
    public void Configure(EntityTypeBuilder<RewardEntity> builder)
    {
        builder.ToTable("rewards", table =>
        {
            table.HasCheckConstraint(
                "CK_rewards_IntensityLevel",
                "\"IntensityLevel\" IS NULL OR (\"IntensityLevel\" BETWEEN 1 AND 4)");
            table.HasCheckConstraint(
                "CK_rewards_RoundNumber",
                "\"RoundNumber\" IS NULL OR \"RoundNumber\" > 0");
        });

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Text).IsRequired();
        builder.Property(r => r.Action).HasMaxLength(50).IsRequired();
        builder.Property(r => r.Location).HasMaxLength(50).IsRequired();
        builder.Property(r => r.TemplateId).HasMaxLength(100);
        builder.Property(r => r.CatalogVersion).HasMaxLength(30);
        builder.Property(r => r.ExecutionType).HasMaxLength(30);
        builder.Property(r => r.ExecutionValue).HasMaxLength(100);

        builder.HasIndex(r => r.GameSessionId);
        builder.HasIndex(r => new { r.GameSessionId, r.RoundNumber });

        builder.HasOne(r => r.GameSession)
            .WithMany()
            .HasForeignKey(r => r.GameSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
