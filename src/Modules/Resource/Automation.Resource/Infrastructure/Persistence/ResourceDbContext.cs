using Automation.SharedKernel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Automation.Resource.Domain;

namespace Automation.Resource.Infrastructure.Persistence;

internal class ResourceDbContext : DbContext
{
    public ResourceDbContext(DbContextOptions<ResourceDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Workspace> Workspaces => Set<Domain.Entities.Workspace>();
    public DbSet<Domain.Entities.ResourceItem> ResourceItems => Set<Domain.Entities.ResourceItem>();
    public DbSet<Domain.Entities.ResourceVersion> ResourceVersions => Set<Domain.Entities.ResourceVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("resource");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ResourceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
