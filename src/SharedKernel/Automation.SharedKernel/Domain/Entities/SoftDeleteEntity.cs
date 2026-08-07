using Automation.SharedKernel.Domain.Interfaces;

namespace Automation.SharedKernel.Domain.Entities;

public class SoftDeleteEntity<TId>: Entity<TId>, ISoftDelete where TId : notnull
{
    public bool IsDeleted => DeletedAt.HasValue;
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

