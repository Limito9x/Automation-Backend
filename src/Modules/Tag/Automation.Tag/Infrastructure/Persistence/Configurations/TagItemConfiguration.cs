using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Tag.Infrastructure.Persistence.Configurations;

public class TagItemConfiguration : IEntityTypeConfiguration<Domain.Entities.TagItem>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.TagItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.TagCategory)
            .WithMany()
            .HasForeignKey(x => x.TagCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Color)
            .HasMaxLength(50);

        builder.HasIndex(x => new { x.TagCategoryId, x.Name })
            .IsUnique();
    }
}

