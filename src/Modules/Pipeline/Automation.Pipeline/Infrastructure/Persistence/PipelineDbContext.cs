using Automation.Pipeline.Domain;
using Automation.SharedKernel.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Automation.Pipeline.Infrastructure.Persistence;

public class PipelineDbContext : DbContext
{
    public PipelineDbContext(DbContextOptions<PipelineDbContext> options)
        : base(options) { }

    public DbSet<Domain.Entities.NodeDefinition> NodeDefinitions =>
        Set<Domain.Entities.NodeDefinition>();
    public DbSet<Domain.Entities.Pipeline> Pipelines => Set<Domain.Entities.Pipeline>();
    public DbSet<Domain.Entities.PipelineNode> PipelineNodes => Set<Domain.Entities.PipelineNode>();
    public DbSet<Domain.Entities.PipelineEdge> PipelineEdges => Set<Domain.Entities.PipelineEdge>();
    public DbSet<Domain.Entities.PipelineInput> PipelineInputs => Set<Domain.Entities.PipelineInput>();
    public DbSet<Domain.Entities.PipelineOutput> PipelineOutputs => Set<Domain.Entities.PipelineOutput>();
    public DbSet<Domain.Entities.PipelineExecution> PipelineExecutions =>
        Set<Domain.Entities.PipelineExecution>();
    public DbSet<Domain.Entities.NodeExecution> NodeExecutions =>
        Set<Domain.Entities.NodeExecution>();
    public DbSet<Domain.Entities.Workflow> Workflows => Set<Domain.Entities.Workflow>();
    public DbSet<Domain.Entities.WorkflowNode> WorkflowNodes => Set<Domain.Entities.WorkflowNode>();
    public DbSet<Domain.Entities.WorkflowEdge> WorkflowEdges => Set<Domain.Entities.WorkflowEdge>();
    public DbSet<Domain.Entities.WorkflowExecution> WorkflowExecutions =>
        Set<Domain.Entities.WorkflowExecution>();
    public DbSet<Domain.Entities.WorkflowNodeExecution> WorkflowNodeExecutions =>
        Set<Domain.Entities.WorkflowNodeExecution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("pipeline");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PipelineDbContext).Assembly);
        modelBuilder.ApplySharedKernelConfigurations();
        base.OnModelCreating(modelBuilder);
    }
}
