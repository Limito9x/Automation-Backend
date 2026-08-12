using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Resource.Infrastructure.Persistence.Configurations;

public class ResourceVersionConfiguration : IEntityTypeConfiguration<Domain.Entities.ResourceVersion>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ResourceVersion> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.VersionNo)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.HasOne(x => x.Resource)
            .WithMany()
            .HasForeignKey(x => x.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ResourceId, x.VersionNo })
            .IsUnique();
    }
}
