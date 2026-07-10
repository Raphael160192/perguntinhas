using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Infrastructure.Persistence.Configurations;

public class QuestionEntityConfiguration : IEntityTypeConfiguration<QuestionEntity>
{
    public void Configure(EntityTypeBuilder<QuestionEntity> builder)
    {
        builder.ToTable("questions");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Text).IsRequired();
        builder.Property(q => q.Theme).HasMaxLength(100).IsRequired();

        builder.HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
