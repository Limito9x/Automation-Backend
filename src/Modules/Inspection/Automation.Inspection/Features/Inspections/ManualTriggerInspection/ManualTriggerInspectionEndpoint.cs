using Automation.Inspection.Shared.Dtos;

namespace Automation.Inspection.Features.Inspections.ManualTriggerInspection;

public class ManualTriggerInspectionEndpoint(IMessageBus bus)
    : Endpoint<ManualTriggerInspectionCommand, IReadOnlyList<InspectionDto>>
{
    public override void Configure()
    {
        Post("/manual-trigger");
        Group<InspectionsGroup>();
        Permissions(P.Inspection.Create);
    }

    public override async Task HandleAsync(ManualTriggerInspectionCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<InspectionDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
