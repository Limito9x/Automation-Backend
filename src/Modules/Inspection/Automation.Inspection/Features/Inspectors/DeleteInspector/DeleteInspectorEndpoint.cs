namespace Automation.Inspection.Features.Inspectors.DeleteInspector;

public class DeleteInspectorEndpoint(IMessageBus bus) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/{id:guid}");
        Group<InspectorsGroup>();
        Permissions(P.Inspector.Delete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new DeleteInspectorCommand(id);
        var result = await bus.InvokeAsync<Result>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
