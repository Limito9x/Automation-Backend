using Automation.Inspection.Shared.Dtos;

namespace Automation.Inspection.Features.Inspectors.CreateInspector;

public class CreateInspectorEndpoint(IMessageBus bus) : Endpoint<CreateInspectorCommand, InspectorDto>
{
    public override void Configure()
    {
        Post("/projects/{projectId:guid}/inspectors");
        Permissions(P.Inspector.Create);
    }

    public override async Task HandleAsync(CreateInspectorCommand req, CancellationToken ct)
    {
        var projectId = Route<Guid>("projectId");
        var command = req with { ProjectId = projectId };
        var result = await bus.InvokeAsync<Result<InspectorDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
