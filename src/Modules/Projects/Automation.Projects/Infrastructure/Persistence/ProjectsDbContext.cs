using Automation.SharedKernel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Automation.Projects.Domain;

namespace Automation.Projects.Infrastructure.Persistence;

internal class ProjectsDbContext : DbContext
{
    public ProjectsDbContext(DbContextOptions<ProjectsDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Project> Projects => Set<Domain.Entities.Project>();
    public DbSet<Domain.Entities.ProjectMember> ProjectMembers => Set<Domain.Entities.ProjectMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("projects");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
