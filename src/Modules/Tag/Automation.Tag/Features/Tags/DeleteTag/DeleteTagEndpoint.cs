namespace Automation.Tag.Features.Tags.DeleteTag;

public class DeleteTagEndpoint(IMessageBus bus) : Endpoint<DeleteTagCommand>
{
    public override void Configure()
    {
        Delete("/tags/{id:guid}");
        Description(x => x.WithTags("Tags"));
        Permissions(P.Tag.Delete);
    }

    public override async Task HandleAsync(DeleteTagCommand command, CancellationToken ct)
    {
        command = command with { Id = Route<Guid>("id") };
        var result = await bus.InvokeAsync<Result>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}