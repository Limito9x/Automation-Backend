using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Tag.Infrastructure.Persistence.Configurations;

public class TagGroupConfiguration : IEntityTypeConfiguration<Domain.Entities.TagGroup>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.TagGroup> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Scope).IsRequired().HasMaxLength(100);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

        // Trong 1 project, scope và name không được trùng nhau
        builder
            .HasIndex(x => new
            {
                x.ProjectId,
                x.Scope,
                x.Name,
            })
            .IsUnique();
    }
}
