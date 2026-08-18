using Automation.Tag.Shared.Dtos;

namespace Automation.Tag.Features.Tags.GetTags;

public class GetTagsEndpoint(IMessageBus bus) : Endpoint<GetTagsQuery, IReadOnlyList<TagItemDto>>
{
    public override void Configure()
    {
        Get("/tags");
        Description(x => x.WithTags("Tags"));
        Permissions(P.Tag.GetAll);
        AllowAnonymous(); // Public read
    }

    public override async Task HandleAsync(GetTagsQuery req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<TagItemDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}