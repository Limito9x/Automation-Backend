using Automation.Content.Shared.Dtos;

namespace Automation.Content.Features.ContentItems.GetContentItems;

internal class GetContentItemsEndpoint(IMessageBus bus)
    : Endpoint<GetContentItemsQuery, PagedResult<ContentItemDto>>
{
    public override void Configure()
    {
        Get("/");
        Group<ContentItemsGroup>();
        Permissions(P.ContentItem.GetAll);
        Description(x => x.WithName("GetContentItems"));
    }

    public override async Task HandleAsync(
        GetContentItemsQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<PagedResult<ContentItemDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
