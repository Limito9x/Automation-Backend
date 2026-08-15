using Automation.SharedKernel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Automation.Agent.Domain.Entities;

namespace Automation.Agent.Infrastructure.Persistence;

public class AgentDbContext : DbContext
{
    public AgentDbContext(DbContextOptions<AgentDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Agent> Agents => Set<Domain.Entities.Agent>();
    public DbSet<AgentPlatformConfig> AgentPlatformConfigs => Set<AgentPlatformConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("agent");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgentDbContext).Assembly);
        modelBuilder.ApplySharedKernelConfigurations();
        base.OnModelCreating(modelBuilder);
    }
}

