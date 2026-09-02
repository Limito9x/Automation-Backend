namespace Automation.Pipeline.Features.Workflows.DeleteWorkflow;

public class DeleteWorkflowEndpoint(IMessageBus bus) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("{id:guid}");
        Group<WorkflowsGroup>();
        Permissions(P.Workflow.Delete);
        Description(d => d
            .Produces(200)
            .Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result>(new DeleteWorkflowCommand(id), ct);
        await this.SendResultAsync(result, ct);
    }
}
