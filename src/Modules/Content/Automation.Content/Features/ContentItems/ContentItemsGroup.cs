using FastEndpoints;

namespace Automation.Content.Features.ContentItems;

internal sealed class ContentItemsGroup : Group
{
    public ContentItemsGroup()
    {
        Configure("/contentitems", ep =>
        {
            ep.Description(b => b.WithTags("ContentItems"));
        });
    }
}
