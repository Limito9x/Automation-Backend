using Automation.SharedKernel.Domain.Interfaces;

namespace Automation.SharedKernel.Domain.Entities;

public abstract class BaseEntity<TId> : ISoftDelete, IAuditable where TId : notnull
{
    public TId Id { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}



