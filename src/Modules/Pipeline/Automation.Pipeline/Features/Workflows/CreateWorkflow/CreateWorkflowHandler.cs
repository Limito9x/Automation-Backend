using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Features.Workflows.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Workflows.CreateWorkflow;

[Transactional(typeof(PipelineDbContext))]
public class CreateWorkflowHandler(PipelineDbContext db)
{
    public async Task<Result<WorkflowSummaryDto>> HandleAsync(
        CreateWorkflowCommand command,
        CancellationToken ct
    )
    {
        var exists = await db.Workflows.AnyAsync(
            x => x.ProjectId == command.ProjectId && x.Name.ToLower() == command.Name.Trim().ToLower(),
            ct
        );

        if (exists)
        {
            return Result.Fail<WorkflowSummaryDto>($"A workflow with name '{command.Name}' already exists in this project.");
        }

        var workflow = new Workflow(command.ProjectId, command.Name.Trim(), command.Description?.Trim());
        db.Workflows.Add(workflow);

        // Auto add initial EventTrigger node
        var triggerNode = new WorkflowNode(
            Guid.NewGuid(),
            workflow.Id,
            "OnResourceCreated",
            WorkflowNodeKind.EventTrigger,
            80,
            150,
            null
        );
        db.WorkflowNodes.Add(triggerNode);

        await db.SaveChangesAsync(ct);

        var dto = new WorkflowSummaryDto(
            workflow.Id,
            workflow.ProjectId,
            workflow.Name,
            workflow.Description,
            workflow.IsActive,
            1,
            0,
            workflow.CreatedAt,
            workflow.UpdatedAt
        );

        return Result.Ok(dto);
    }
}
