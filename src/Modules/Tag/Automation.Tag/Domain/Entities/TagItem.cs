namespace Automation.Tag.Domain.Entities;

public class TagItem : BaseEntity<Guid>
{
    public Guid TagCategoryId { get; private set; }
    public TagCategory TagCategory { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Color { get; private set; }

    protected TagItem() { }

    public TagItem(Guid tagCategoryId, string name, string? color = null)
    {
        Id = Guid.NewGuid();
        TagCategoryId = tagCategoryId;
        Name = name;
        Color = color;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

