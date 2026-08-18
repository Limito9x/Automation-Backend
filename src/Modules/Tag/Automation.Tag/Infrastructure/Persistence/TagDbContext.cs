using Automation.SharedKernel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Automation.Tag.Domain;

namespace Automation.Tag.Infrastructure.Persistence;

public class TagDbContext : DbContext
{
    public TagDbContext(DbContextOptions<TagDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.TagGroup> TagGroups => Set<Domain.Entities.TagGroup>();
    public DbSet<Domain.Entities.TagItem> TagItems => Set<Domain.Entities.TagItem>();
    public DbSet<Domain.Entities.TagLink> TagLinks => Set<Domain.Entities.TagLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("tag");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TagDbContext).Assembly);
        modelBuilder.ApplySharedKernelConfigurations();
        base.OnModelCreating(modelBuilder);
    }
}