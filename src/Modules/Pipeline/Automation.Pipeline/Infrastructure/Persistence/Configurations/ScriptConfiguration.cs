using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Pipeline.Infrastructure.Persistence.Configurations;

public class ScriptConfiguration : IEntityTypeConfiguration<Domain.Entities.Script>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Script> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(x => x.WorkerType)
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(x => x.ScriptPath)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(x => x.ParamsConfig)
            .HasColumnType("jsonb")
            .IsRequired();
            
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

