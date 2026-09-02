using Automation.Pipeline.Features.Workflows.Dtos;

namespace Automation.Pipeline.Features.Workflows.GetWorkflowGraph;

public class GetWorkflowGraphEndpoint(IMessageBus bus) : EndpointWithoutRequest<WorkflowGraphDto>
{
    public override void Configure()
    {
        Get("{id:guid}/graph");
        Group<WorkflowsGroup>();
        Permissions(P.Workflow.GetById);
        Description(d => d
            .Produces<WorkflowGraphDto>(200)
            .Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result<WorkflowGraphDto>>(new GetWorkflowGraphQuery(id), ct);
        await this.SendResultAsync(result, ct);
    }
}
