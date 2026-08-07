using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Resource.Infrastructure.Persistence.Configurations;

public class StorageConfiguration : IEntityTypeConfiguration<Domain.Entities.Storage>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Storage> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(x => x.Kind)
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(x => x.RootPath)
            .HasMaxLength(500);
            
        builder.HasIndex(x => new { x.ProjectId, x.Name }).IsUnique();
    }
}
