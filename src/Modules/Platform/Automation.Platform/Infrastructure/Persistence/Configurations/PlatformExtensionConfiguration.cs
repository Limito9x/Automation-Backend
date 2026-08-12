using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Platform.Infrastructure.Persistence.Configurations;

public class PlatformExtensionConfiguration : IEntityTypeConfiguration<Domain.Entities.PlatformExtension>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.PlatformExtension> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Extension)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(x => x.Platform)
            .WithMany(x => x.Extensions)
            .HasForeignKey(x => x.PlatformId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.PlatformId, x.Extension })
            .IsUnique();
    }
}
