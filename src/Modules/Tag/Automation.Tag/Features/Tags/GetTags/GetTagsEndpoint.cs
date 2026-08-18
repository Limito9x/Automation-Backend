using Automation.Tag.Shared.Dtos;

namespace Automation.Tag.Features.Tags.GetTags;

public class GetTagsEndpoint(IMessageBus bus) : EndpointWithoutRequest<IReadOnlyList<TagItemDto>>
{
    public override void Configure()
    {
        Get("/tags");
        Permissions(P.Tag.GetAll);
        AllowAnonymous(); // Public read
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var tagGroupIdStr = Query<string?>("tagGroupId");
        Guid? tagGroupId = tagGroupIdStr is not null ? Guid.Parse(tagGroupIdStr) : null;
        var query = new GetTagsQuery(tagGroupId);
        var result = await bus.InvokeAsync<Result<IReadOnlyList<TagItemDto>>>(query, ct);
        await this.SendResultAsync(result, ct);
    }
}