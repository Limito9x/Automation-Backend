using Automation.SharedKernel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Automation.Inspection.Domain;

namespace Automation.Inspection.Infrastructure.Persistence;

internal class InspectionDbContext : DbContext
{
    public InspectionDbContext(DbContextOptions<InspectionDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Inspector> Inspectors => Set<Domain.Entities.Inspector>();
    public DbSet<Domain.Entities.ContentTypeInspectorConfig> ContentTypeInspectorConfigs => Set<Domain.Entities.ContentTypeInspectorConfig>();
    public DbSet<Domain.Entities.InspectionRecord> InspectionRecords => Set<Domain.Entities.InspectionRecord>();
    public DbSet<Domain.Entities.InspectionItem> InspectionItems => Set<Domain.Entities.InspectionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inspection");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InspectionDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
