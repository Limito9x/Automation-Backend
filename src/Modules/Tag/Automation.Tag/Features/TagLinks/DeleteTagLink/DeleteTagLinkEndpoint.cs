namespace Automation.Tag.Features.TagLinks.DeleteTagLink;

public class DeleteTagLinkEndpoint(IMessageBus bus) : Endpoint<DeleteTagLinkCommand>
{
    public override void Configure()
    {
        Delete("/tag-links/{id:guid}");
        Permissions(P.TagLink.Delete);
    }

    public override async Task HandleAsync(DeleteTagLinkCommand command, CancellationToken ct)
    {
        command = command with { Id = Route<Guid>("id") };
        var result = await bus.InvokeAsync<Result>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}