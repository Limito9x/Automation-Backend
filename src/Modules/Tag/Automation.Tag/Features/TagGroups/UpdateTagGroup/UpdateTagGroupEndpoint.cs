using Automation.Tag.Shared.Dtos;

namespace Automation.Tag.Features.TagGroups.UpdateTagGroup;

public class UpdateTagGroupEndpoint(IMessageBus bus) : Endpoint<UpdateTagGroupCommand, TagGroupDto>
{
    public override void Configure()
    {
        Put("/tag-groups/{id:guid}");
        Description(x => x.WithTags("Tags"));
        Permissions(P.TagGroup.Update);
    }

    public override async Task HandleAsync(UpdateTagGroupCommand command, CancellationToken ct)
    {
        command = command with { Id = Route<Guid>("id") };
        var result = await bus.InvokeAsync<Result<TagGroupDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}