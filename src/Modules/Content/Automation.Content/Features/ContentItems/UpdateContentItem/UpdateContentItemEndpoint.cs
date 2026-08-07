using Automation.Content.Shared.Dtos;

namespace Automation.Content.Features.ContentItems.UpdateContentItem;

internal class UpdateContentItemEndpoint(IMessageBus bus)
    : Endpoint<UpdateContentItemCommand, ContentItemDto>
{
    public override void Configure()
    {
        Put("{Id:guid}");
        Group<ContentItemsGroup>();
        Permissions(P.ContentItem.Update);
        Description(x => x.WithName("UpdateContentItem"));
    }

    public override async Task HandleAsync(
        UpdateContentItemCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<ContentItemDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
