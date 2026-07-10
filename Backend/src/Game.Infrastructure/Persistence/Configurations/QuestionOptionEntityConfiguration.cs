using Game.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Game.Infrastructure.Persistence.Configurations;

public class QuestionOptionEntityConfiguration : IEntityTypeConfiguration<QuestionOptionEntity>
{
    public void Configure(EntityTypeBuilder<QuestionOptionEntity> builder)
    {
        builder.ToTable("question_options");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Text).IsRequired();
    }
}
