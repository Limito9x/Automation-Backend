using FastEndpoints;

namespace Automation.Content.Features.ContentItems;

internal sealed class ContentItemsGroup : Group
{
    public ContentItemsGroup()
    {
        Configure("", ep =>
        {
            ep.Description(b => b.WithTags("ContentItems"));
        });
    }
}
