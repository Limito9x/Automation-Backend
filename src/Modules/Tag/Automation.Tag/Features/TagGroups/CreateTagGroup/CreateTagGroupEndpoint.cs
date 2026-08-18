using Automation.Tag.Shared.Dtos;

namespace Automation.Tag.Features.TagGroups.CreateTagGroup;

public class CreateTagGroupEndpoint(IMessageBus bus) : Endpoint<CreateTagGroupCommand, TagGroupDto>
{
    public override void Configure()
    {
        Post("/tag-groups");
        Permissions(P.TagGroup.Create);
    }

    public override async Task HandleAsync(CreateTagGroupCommand command, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<TagGroupDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}