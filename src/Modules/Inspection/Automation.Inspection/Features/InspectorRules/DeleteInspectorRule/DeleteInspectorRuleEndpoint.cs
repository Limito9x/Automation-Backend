namespace Automation.Inspection.Features.InspectorRules.DeleteInspectorRule;

public class DeleteInspectorRuleEndpoint(IMessageBus bus) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/{id:guid}");
        Group<InspectorRulesGroup>();
        Permissions(P.InspectorRule.Delete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new DeleteInspectorRuleCommand(id);
        var result = await bus.InvokeAsync<Result>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
