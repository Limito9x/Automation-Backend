using System.Text.Json;
using Automation.SharedKernel.Domain.Interfaces;

namespace Automation.Content.Domain.Entities;

public class ContentItem : BaseEntity<Guid>, IAuditTrackable
{
    public Guid ContentTypeId { get; private set; }
    public ContentType ContentType { get; private set; } = null!;
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    protected ContentItem() { }

    public ContentItem(Guid contentTypeId, Guid projectId, string name)
    {
        Id = Guid.NewGuid();
        ContentTypeId = contentTypeId;
        ProjectId = projectId;
        Name = name;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string name)
    {
        Name = name;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
