using Automation.Inspection.Shared.Dtos;

namespace Automation.Inspection.Features.Inspectors.PublishInspectorVersion;

public class PublishInspectorVersionEndpoint(IMessageBus bus) : Endpoint<PublishInspectorVersionCommand, InspectorVersionDto>
{
    public override void Configure()
    {
        Put("/{id:guid}/versions/{versionId:guid}/publish");
        Group<InspectorsGroup>();
        Permissions(P.Inspector.Update);
    }

    public override async Task HandleAsync(PublishInspectorVersionCommand req, CancellationToken ct)
    {
        var versionId = Route<Guid>("versionId");
        var command = req with { VersionId = versionId };
        var result = await bus.InvokeAsync<Result<InspectorVersionDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
