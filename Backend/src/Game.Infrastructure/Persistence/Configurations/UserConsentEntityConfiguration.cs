using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Infrastructure.Persistence.Configurations;

public class UserConsentEntityConfiguration : IEntityTypeConfiguration<UserConsentEntity>
{
    public void Configure(EntityTypeBuilder<UserConsentEntity> builder)
    {
        builder.ToTable("user_consents");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ConsentType).HasMaxLength(30).IsRequired();
        builder.Property(c => c.PolicyVersion).HasMaxLength(20).IsRequired();

        builder.HasIndex(c => c.UserId);
    }
}
