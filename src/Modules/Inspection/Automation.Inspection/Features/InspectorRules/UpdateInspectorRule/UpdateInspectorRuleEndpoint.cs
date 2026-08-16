using Automation.Inspection.Shared.Dtos;

namespace Automation.Inspection.Features.InspectorRules.UpdateInspectorRule;

public class UpdateInspectorRuleEndpoint(IMessageBus bus) : Endpoint<UpdateInspectorRuleCommand, InspectorRuleDto>
{
    public override void Configure()
    {
        Put("/{id:guid}");
        Group<InspectorRulesGroup>();
        Permissions(P.InspectorRule.Update);
    }

    public override async Task HandleAsync(UpdateInspectorRuleCommand req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = req with { Id = id };
        var result = await bus.InvokeAsync<Result<InspectorRuleDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
