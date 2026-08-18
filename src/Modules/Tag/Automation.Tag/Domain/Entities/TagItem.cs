namespace Automation.Tag.Domain.Entities;

public class TagItem : BaseEntity<Guid>
{
    public Guid TagGroupId { get; private set; }
    public TagGroup TagGroup { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Color { get; private set; }

    protected TagItem() { }

    public TagItem(Guid tagGroupId, string name, string? color = null)
    {
        Id = Guid.NewGuid();
        TagGroupId = tagGroupId;
        Name = name;
        Color = color;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string name, string? color)
    {
        Name = name;
        Color = color;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}