using Automation.Inspection.Shared.Dtos;

namespace Automation.Inspection.Features.Inspectors.GetInspectors;

public class GetInspectorsEndpoint(IMessageBus bus) : EndpointWithoutRequest<IReadOnlyList<InspectorDto>>
{
    public override void Configure()
    {
        Get("/projects/{projectId:guid}/inspectors");
        Permissions(P.Inspector.GetAll);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var projectId = Route<Guid>("projectId");
        var query = new GetInspectorsQuery(projectId);
        var result = await bus.InvokeAsync<Result<IReadOnlyList<InspectorDto>>>(query, ct);
        await this.SendResultAsync(result, ct);
    }
}
