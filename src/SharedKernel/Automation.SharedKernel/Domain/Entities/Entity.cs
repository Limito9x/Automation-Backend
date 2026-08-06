namespace Automation.SharedKernel.Domain.Entities;

public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected init; } = default!;
    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    public override int GetHashCode() => Id.GetHashCode();
}

public abstract class Entity : Entity<Guid>
{
    protected Entity()
    {
        Id = Guid.CreateVersion7();
    }
}
