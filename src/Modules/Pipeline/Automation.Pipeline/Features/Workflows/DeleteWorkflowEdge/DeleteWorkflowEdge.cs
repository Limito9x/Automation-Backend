using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Workflows.DeleteWorkflowEdge;

public record DeleteWorkflowEdgeCommand(
    Guid WorkflowId,
    Guid EdgeId
);

[Transactional(typeof(PipelineDbContext))]
public class DeleteWorkflowEdgeHandler(PipelineDbContext db)
{
    public async Task<Result> HandleAsync(
        DeleteWorkflowEdgeCommand command,
        CancellationToken ct
    )
    {
        var edge = await db.WorkflowEdges.FirstOrDefaultAsync(
            x => x.Id == command.EdgeId && x.WorkflowId == command.WorkflowId,
            ct
        );

        if (edge == null)
        {
            return Result.Fail($"WorkflowEdge with ID '{command.EdgeId}' not found in Workflow '{command.WorkflowId}'.");
        }

        db.WorkflowEdges.Remove(edge);
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

public class DeleteWorkflowEdgeEndpoint(IMessageBus bus) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("{workflowId:guid}/edges/{edgeId:guid}");
        Group<WorkflowsGroup>();
        Permissions(P.Workflow.Update);
        Description(d => d
            .Produces(200)
            .Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var workflowId = Route<Guid>("workflowId");
        var edgeId = Route<Guid>("edgeId");
        var result = await bus.InvokeAsync<Result>(new DeleteWorkflowEdgeCommand(workflowId, edgeId), ct);
        await this.SendResultAsync(result, ct);
    }
}
