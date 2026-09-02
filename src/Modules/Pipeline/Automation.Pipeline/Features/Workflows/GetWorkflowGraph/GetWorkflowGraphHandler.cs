using Automation.Pipeline.Features.Workflows.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Workflows.GetWorkflowGraph;

[NonTransactional]
public class GetWorkflowGraphHandler(PipelineDbContext db)
{
    public async Task<Result<WorkflowGraphDto>> HandleAsync(
        GetWorkflowGraphQuery query,
        CancellationToken ct
    )
    {
        var workflow = await db.Workflows
            .AsNoTracking()
            .Include(x => x.Nodes)
            .Include(x => x.Edges)
            .FirstOrDefaultAsync(x => x.Id == query.Id, ct);

        if (workflow == null)
        {
            return Result.Fail<WorkflowGraphDto>($"Workflow with ID '{query.Id}' not found.");
        }

        var dto = new WorkflowGraphDto(
            workflow.Id,
            workflow.ProjectId,
            workflow.Name,
            workflow.Description,
            workflow.IsActive,
            workflow.CreatedAt,
            workflow.UpdatedAt,
            workflow.Nodes.Select(n => new WorkflowNodeDto(
                n.Id,
                n.WorkflowId,
                n.RefId,
                n.Kind,
                n.Position,
                n.Config
            )).ToList(),
            workflow.Edges.Select(e => new WorkflowEdgeDto(
                e.Id,
                e.WorkflowId,
                e.SourceWorkflowNodeId,
                e.SourcePin,
                e.TargetWorkflowNodeId,
                e.TargetPin
            )).ToList()
        );

        return Result.Ok(dto);
    }
}
