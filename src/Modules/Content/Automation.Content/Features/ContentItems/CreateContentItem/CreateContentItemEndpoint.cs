using Automation.Content.Shared.Dtos;

namespace Automation.Content.Features.ContentItems.CreateContentItem;

internal class CreateContentItemEndpoint(IMessageBus bus)
    : Endpoint<CreateContentItemCommand, ContentItemDto>
{
    public override void Configure()
    {
        Post("/"); // Change this method/route accordingly
        Group<ContentItemsGroup>();
        Permissions(P.ContentItem.Create);
        Description(x => x.WithName("CreateContentItem"));
    }

    public override async Task HandleAsync(
        CreateContentItemCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<ContentItemDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
