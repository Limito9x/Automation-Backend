using Automation.Inspection.Shared.Dtos;

namespace Automation.Inspection.Features.Inspectors.CreateInspectorVersion;

public class CreateInspectorVersionEndpoint(IMessageBus bus) : Endpoint<CreateInspectorVersionCommand, InspectorVersionDto>
{
    public override void Configure()
    {
        Post("/{id:guid}/versions");
        Group<InspectorsGroup>();
        Permissions(P.Inspector.Create);
    }

    public override async Task HandleAsync(CreateInspectorVersionCommand req, CancellationToken ct)
    {
        var inspectorId = Route<Guid>("id");
        var command = req with { InspectorId = inspectorId };
        var result = await bus.InvokeAsync<Result<InspectorVersionDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
