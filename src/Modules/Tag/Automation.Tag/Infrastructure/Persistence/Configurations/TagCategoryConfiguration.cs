using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Tag.Infrastructure.Persistence.Configurations;

public class TagCategoryConfiguration : IEntityTypeConfiguration<Domain.Entities.TagCategory>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.TagCategory> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.HasIndex(x => new { x.ProjectId, x.Name }).IsUnique();
    }
}

