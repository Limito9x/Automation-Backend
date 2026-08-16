using Automation.Inspection.Shared.Dtos;

namespace Automation.Inspection.Features.Inspectors.GetInspectorById;

public class GetInspectorByIdEndpoint(IMessageBus bus) : EndpointWithoutRequest<InspectorDto>
{
    public override void Configure()
    {
        Get("/{id:guid}");
        Group<InspectorsGroup>();
        Permissions(P.Inspector.GetById);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var query = new GetInspectorByIdQuery(id);
        var result = await bus.InvokeAsync<Result<InspectorDto>>(query, ct);
        await this.SendResultAsync(result, ct);
    }
}
