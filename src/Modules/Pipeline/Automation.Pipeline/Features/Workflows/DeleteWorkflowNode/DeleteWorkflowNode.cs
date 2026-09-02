using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Workflows.DeleteWorkflowNode;

public record DeleteWorkflowNodeCommand(
    Guid WorkflowId,
    Guid NodeId
);

[Transactional(typeof(PipelineDbContext))]
public class DeleteWorkflowNodeHandler(PipelineDbContext db)
{
    public async Task<Result> HandleAsync(
        DeleteWorkflowNodeCommand command,
        CancellationToken ct
    )
    {
        var node = await db.WorkflowNodes.FirstOrDefaultAsync(
            x => x.Id == command.NodeId && x.WorkflowId == command.WorkflowId,
            ct
        );

        if (node == null)
        {
            return Result.Fail($"WorkflowNode with ID '{command.NodeId}' not found in Workflow '{command.WorkflowId}'.");
        }

        // Delete connected edges
        var connectedEdges = await db.WorkflowEdges
            .Where(x => x.WorkflowId == command.WorkflowId &&
                        (x.SourceWorkflowNodeId == command.NodeId || x.TargetWorkflowNodeId == command.NodeId))
            .ToListAsync(ct);

        if (connectedEdges.Count > 0)
        {
            db.WorkflowEdges.RemoveRange(connectedEdges);
        }

        db.WorkflowNodes.Remove(node);
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

public class DeleteWorkflowNodeEndpoint(IMessageBus bus) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("{workflowId:guid}/nodes/{nodeId:guid}");
        Group<WorkflowsGroup>();
        Permissions(P.Workflow.Update);
        Description(d => d
            .Produces(200)
            .Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var workflowId = Route<Guid>("workflowId");
        var nodeId = Route<Guid>("nodeId");
        var result = await bus.InvokeAsync<Result>(new DeleteWorkflowNodeCommand(workflowId, nodeId), ct);
        await this.SendResultAsync(result, ct);
    }
}
