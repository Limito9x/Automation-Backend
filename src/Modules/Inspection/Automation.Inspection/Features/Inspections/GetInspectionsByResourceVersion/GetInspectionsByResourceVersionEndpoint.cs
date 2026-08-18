using Automation.Inspection.Shared.Dtos;

namespace Automation.Inspection.Features.Inspections.GetInspectionsByResourceVersion;

public class GetInspectionsByResourceVersionEndpoint(IMessageBus bus)
    : EndpointWithoutRequest<IReadOnlyList<InspectionDetailDto>>
{
    public override void Configure()
    {
        Get("resource-versions/{resourceVersionId:guid}/inspections");
        Description(x => x.WithTags("Inspections"));
        Description(x => x.WithName("GetInspectionsByResourceVersion"));
        Permissions(P.Inspection.GetAll);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var resourceVersionId = Route<Guid>("resourceVersionId");
        var query = new GetInspectionsByResourceVersionQuery(resourceVersionId);
        var result = await bus.InvokeAsync<Result<IReadOnlyList<InspectionDetailDto>>>(query, ct);
        await this.SendResultAsync(result, ct);
    }
}
