using Automation.Inspection.Shared.Dtos;

namespace Automation.Inspection.Features.Inspectors.UpdateInspector;

public class UpdateInspectorEndpoint(IMessageBus bus) : Endpoint<UpdateInspectorCommand, InspectorDto>
{
    public override void Configure()
    {
        Put("/{id:guid}");
        Group<InspectorsGroup>();
        Permissions(P.Inspector.Update);
    }

    public override async Task HandleAsync(UpdateInspectorCommand req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = req with { Id = id };
        var result = await bus.InvokeAsync<Result<InspectorDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
