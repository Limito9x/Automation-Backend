using Automation.Inspection.Shared.Dtos;

namespace Automation.Inspection.Features.InspectorRules.CreateInspectorRule;

public class CreateInspectorRuleEndpoint(IMessageBus bus) : Endpoint<CreateInspectorRuleCommand, InspectorRuleDto>
{
    public override void Configure()
    {
        Post("/projects/{projectId:guid}/inspector-rules");
        Permissions(P.InspectorRule.Create);
    }

    public override async Task HandleAsync(CreateInspectorRuleCommand req, CancellationToken ct)
    {
        var projectId = Route<Guid>("projectId");
        var command = req with { ProjectId = projectId };
        var result = await bus.InvokeAsync<Result<InspectorRuleDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
