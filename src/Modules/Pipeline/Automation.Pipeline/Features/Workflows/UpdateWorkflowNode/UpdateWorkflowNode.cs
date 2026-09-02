using System.Text.Json;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Workflows.UpdateWorkflowNode;

public record UpdateWorkflowNodeCommand(
    Guid WorkflowId,
    Guid NodeId,
    float? PositionX = null,
    float? PositionY = null,
    JsonDocument? Config = null
);

[Transactional(typeof(PipelineDbContext))]
public class UpdateWorkflowNodeHandler(PipelineDbContext db)
{
    public async Task<Result> HandleAsync(
        UpdateWorkflowNodeCommand command,
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

        if (command.PositionX.HasValue && command.PositionY.HasValue)
        {
            node.Update(command.PositionX.Value, command.PositionY.Value);
        }

        if (command.Config != null)
        {
            node.UpdateConfig(command.Config);
        }

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

public class UpdateWorkflowNodeEndpoint(IMessageBus bus) : Endpoint<UpdateWorkflowNodeCommand>
{
    public override void Configure()
    {
        Put("{workflowId:guid}/nodes/{nodeId:guid}");
        Group<WorkflowsGroup>();
        Permissions(P.Workflow.Update);
        Description(d => d
            .Produces(200)
            .Produces(404));
    }

    public override async Task HandleAsync(UpdateWorkflowNodeCommand req, CancellationToken ct)
    {
        var workflowId = Route<Guid>("workflowId");
        var nodeId = Route<Guid>("nodeId");
        var cmd = req with { WorkflowId = workflowId, NodeId = nodeId };
        var result = await bus.InvokeAsync<Result>(cmd, ct);
        await this.SendResultAsync(result, ct);
    }
}
