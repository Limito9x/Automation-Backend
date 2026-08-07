using Automation.SharedKernel.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Automation.Tag.Domain;

namespace Automation.Tag.Infrastructure.Persistence;

internal class TagDbContext : DbContext
{
    public TagDbContext(DbContextOptions<TagDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.TagCategory> TagCategories => Set<Domain.Entities.TagCategory>();
    public DbSet<Domain.Entities.TagItem> TagItems => Set<Domain.Entities.TagItem>();
    public DbSet<Domain.Entities.TagLink> TagLinks => Set<Domain.Entities.TagLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("tag");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TagDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
