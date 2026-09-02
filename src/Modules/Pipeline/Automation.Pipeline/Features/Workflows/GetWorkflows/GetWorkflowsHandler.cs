using Automation.Pipeline.Features.Workflows.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Workflows.GetWorkflows;

[NonTransactional]
public class GetWorkflowsHandler(PipelineDbContext db)
{
    public async Task<Result<List<WorkflowSummaryDto>>> HandleAsync(
        GetWorkflowsQuery query,
        CancellationToken ct
    )
    {
        var workflows = await db.Workflows
            .AsNoTracking()
            .Where(x => x.ProjectId == query.ProjectId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new WorkflowSummaryDto(
                x.Id,
                x.ProjectId,
                x.Name,
                x.Description,
                x.IsActive,
                x.Nodes.Count,
                x.Edges.Count,
                x.CreatedAt,
                x.UpdatedAt
            ))
            .ToListAsync(ct);

        return Result.Ok(workflows);
    }
}
