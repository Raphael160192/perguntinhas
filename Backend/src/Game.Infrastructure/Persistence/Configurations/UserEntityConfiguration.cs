using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Infrastructure.Persistence.Configurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).HasMaxLength(320).IsRequired();
        builder.Property(u => u.GoogleSubject).HasMaxLength(255);
        builder.Property(u => u.AuthProvider).HasMaxLength(20).IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        // Único apenas entre contas que têm GoogleSubject (mesmo padrão do índice
        // filtrado de JoinCode em game_sessions).
        builder.HasIndex(u => u.GoogleSubject)
            .IsUnique()
            .HasFilter("\"GoogleSubject\" IS NOT NULL");

        builder.HasMany(u => u.Consents)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
