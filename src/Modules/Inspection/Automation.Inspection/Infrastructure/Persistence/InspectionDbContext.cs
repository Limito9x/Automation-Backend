using Automation.SharedKernel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Automation.Inspection.Domain;

namespace Automation.Inspection.Infrastructure.Persistence;

public class InspectionDbContext : DbContext
{
    public InspectionDbContext(DbContextOptions<InspectionDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Inspector> Inspectors => Set<Domain.Entities.Inspector>();
    public DbSet<Domain.Entities.InspectorVersion> InspectorVersions => Set<Domain.Entities.InspectorVersion>();
    public DbSet<Domain.Entities.InspectorRule> InspectorRules => Set<Domain.Entities.InspectorRule>();
    public DbSet<Domain.Entities.Inspection> Inspections => Set<Domain.Entities.Inspection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inspection");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InspectionDbContext).Assembly);
        modelBuilder.ApplySharedKernelConfigurations();
        base.OnModelCreating(modelBuilder);
    }
}

