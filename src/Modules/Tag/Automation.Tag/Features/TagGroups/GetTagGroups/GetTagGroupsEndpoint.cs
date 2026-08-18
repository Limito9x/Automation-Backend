using Automation.Tag.Shared.Dtos;

namespace Automation.Tag.Features.TagGroups.GetTagGroups;

public class GetTagGroupsEndpoint(IMessageBus bus)
    : Endpoint<GetTagGroupsQuery, IReadOnlyList<TagGroupDto>>
{
    public override void Configure()
    {
        Get("/tag-groups");
        Permissions(P.TagGroup.GetAll);
    }

    public override async Task HandleAsync(GetTagGroupsQuery req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<TagGroupDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
