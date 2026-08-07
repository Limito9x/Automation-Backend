using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Resource.Infrastructure.Persistence.Configurations;

public class ResourceItemConfiguration : IEntityTypeConfiguration<Domain.Entities.ResourceItem>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.ResourceItem> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasOne(x => x.Storage)
            .WithMany()
            .HasForeignKey(x => x.StorageId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.AssetId);
    }
}
