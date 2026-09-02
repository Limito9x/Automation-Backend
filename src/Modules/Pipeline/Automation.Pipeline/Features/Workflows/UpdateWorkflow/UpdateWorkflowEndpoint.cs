namespace Automation.Pipeline.Features.Workflows.UpdateWorkflow;

public class UpdateWorkflowEndpoint(IMessageBus bus) : Endpoint<UpdateWorkflowCommand>
{
    public override void Configure()
    {
        Put("{id:guid}");
        Group<WorkflowsGroup>();
        Permissions(P.Workflow.Update);
        Description(d => d
            .Produces(200)
            .Produces(400)
            .Produces(404));
    }

    public override async Task HandleAsync(UpdateWorkflowCommand req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var cmd = req with { Id = id };
        var result = await bus.InvokeAsync<Result>(cmd, ct);
        await this.SendResultAsync(result, ct);
    }
}
