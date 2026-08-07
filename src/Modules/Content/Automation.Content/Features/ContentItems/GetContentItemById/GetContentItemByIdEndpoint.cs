using Automation.Content.Shared.Dtos;

namespace Automation.Content.Features.ContentItems.GetContentItemById;

internal class GetContentItemByIdEndpoint(IMessageBus bus)
    : Endpoint<GetContentItemByIdQuery, ContentItemDto>
{
    public override void Configure()
    {
        Get("/{id}");
        Group<ContentItemsGroup>();
        Permissions(P.ContentItem.GetById);
        Description(x => x.WithName("GetContentItemById"));
    }

    public override async Task HandleAsync(
        GetContentItemByIdQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<ContentItemDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
