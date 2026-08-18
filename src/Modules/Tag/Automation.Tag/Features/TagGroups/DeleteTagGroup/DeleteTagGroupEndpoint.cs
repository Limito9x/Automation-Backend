namespace Automation.Tag.Features.TagGroups.DeleteTagGroup;

public class DeleteTagGroupEndpoint(IMessageBus bus) : Endpoint<DeleteTagGroupCommand>
{
    public override void Configure()
    {
        Delete("/tag-groups/{id:guid}");
        Description(x => x.WithTags("Tags"));
        Permissions(P.TagGroup.Delete);
    }

    public override async Task HandleAsync(DeleteTagGroupCommand command, CancellationToken ct)
    {
        command = command with { Id = Route<Guid>("id") };
        var result = await bus.InvokeAsync<Result>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}