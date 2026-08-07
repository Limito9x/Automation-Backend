using Gridify;

namespace Automation.Content.Features.ContentItems.GetContentItems;

public class GetContentItemsQuery : PagedQuery
{
    public Guid? ProjectId { get; set; }
    public Guid? ContentTypeId { get; set; }
}
