using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Tag.Infrastructure.Persistence.Configurations;

public class TagLinkConfiguration : IEntityTypeConfiguration<Domain.Entities.TagLink>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.TagLink> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.Tag)
            .WithMany()
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.EntityType).IsRequired().HasMaxLength(100);

        builder.Property(x => x.EntityId).IsRequired();

        builder.Property(x => x.Metadata).HasColumnType("jsonb");

        builder.Property(x => x.ProjectId).IsRequired();

        // ProjectId bất biến, đánh index tìm nhanh hơn
        builder.HasIndex(x => x.ProjectId);

        builder.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}
