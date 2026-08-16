using Automation.Inspection.Shared.Dtos;

namespace Automation.Inspection.Features.Inspections.TriggerInspection;

public class TriggerInspectionEndpoint(IMessageBus bus) : Endpoint<TriggerInspectionCommand, IReadOnlyList<InspectionDto>>
{
    public override void Configure()
    {
        Post("/trigger");
        Group<InspectionsGroup>();
        Permissions(P.Inspection.Create);
    }

    public override async Task HandleAsync(TriggerInspectionCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<InspectionDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
