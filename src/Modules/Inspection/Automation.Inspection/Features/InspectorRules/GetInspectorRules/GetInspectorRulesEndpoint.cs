using Automation.Inspection.Shared.Dtos;

namespace Automation.Inspection.Features.InspectorRules.GetInspectorRules;

public class GetInspectorRulesEndpoint(IMessageBus bus) : EndpointWithoutRequest<IReadOnlyList<InspectorRuleDto>>
{
    public override void Configure()
    {
        Get("/projects/{projectId:guid}/inspector-rules");
        Permissions(P.InspectorRule.GetAll);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var projectId = Route<Guid>("projectId");
        var query = new GetInspectorRulesQuery(projectId);
        var result = await bus.InvokeAsync<Result<IReadOnlyList<InspectorRuleDto>>>(query, ct);
        await this.SendResultAsync(result, ct);
    }
}
