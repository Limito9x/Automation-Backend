using Automation.SharedKernel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Automation.Pipeline.Domain;

namespace Automation.Pipeline.Infrastructure.Persistence;

internal class PipelineDbContext : DbContext
{
    public PipelineDbContext(DbContextOptions<PipelineDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Script> Scripts => Set<Domain.Entities.Script>();
    public DbSet<Domain.Entities.ToolDefinition> ToolDefinitions => Set<Domain.Entities.ToolDefinition>();
    public DbSet<Domain.Entities.SessionDefinition> SessionDefinitions => Set<Domain.Entities.SessionDefinition>();
    public DbSet<Domain.Entities.NodeDefinition> NodeDefinitions => Set<Domain.Entities.NodeDefinition>();
    public DbSet<Domain.Entities.PipelineItem> PipelineItems => Set<Domain.Entities.PipelineItem>();
    public DbSet<Domain.Entities.PipelineNode> PipelineNodes => Set<Domain.Entities.PipelineNode>();
    public DbSet<Domain.Entities.PipelineEdge> PipelineEdges => Set<Domain.Entities.PipelineEdge>();
    public DbSet<Domain.Entities.PipelineExecution> PipelineExecutions => Set<Domain.Entities.PipelineExecution>();
    public DbSet<Domain.Entities.NodeExecution> NodeExecutions => Set<Domain.Entities.NodeExecution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("pipeline");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PipelineDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
