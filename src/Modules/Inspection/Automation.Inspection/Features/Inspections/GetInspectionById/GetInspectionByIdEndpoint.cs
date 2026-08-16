using Automation.Inspection.Shared.Dtos;

namespace Automation.Inspection.Features.Inspections.GetInspectionById;

public class GetInspectionByIdEndpoint(IMessageBus bus) : EndpointWithoutRequest<InspectionDto>
{
    public override void Configure()
    {
        Get("/{id:guid}");
        Group<InspectionsGroup>();
        Permissions(P.Inspection.GetById);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var query = new GetInspectionByIdQuery(id);
        var result = await bus.InvokeAsync<Result<InspectionDto>>(query, ct);
        await this.SendResultAsync(result, ct);
    }
}
