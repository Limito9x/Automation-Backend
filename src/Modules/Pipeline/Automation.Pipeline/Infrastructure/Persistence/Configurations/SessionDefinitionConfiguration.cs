using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automation.Pipeline.Infrastructure.Persistence.Configurations;

public class SessionDefinitionConfiguration : IEntityTypeConfiguration<Domain.Entities.SessionDefinition>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.SessionDefinition> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(x => x.WorkerType)
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(x => x.Flow)
            .HasColumnType("jsonb")
            .IsRequired();
    }
}

