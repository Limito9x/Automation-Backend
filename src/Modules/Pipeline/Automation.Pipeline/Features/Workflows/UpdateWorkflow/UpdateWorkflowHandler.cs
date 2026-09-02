using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Workflows.UpdateWorkflow;

[Transactional(typeof(PipelineDbContext))]
public class UpdateWorkflowHandler(PipelineDbContext db)
{
    public async Task<Result> HandleAsync(
        UpdateWorkflowCommand command,
        CancellationToken ct
    )
    {
        var workflow = await db.Workflows.FirstOrDefaultAsync(x => x.Id == command.Id, ct);

        if (workflow == null)
        {
            return Result.Fail($"Workflow with ID '{command.Id}' not found.");
        }

        var exists = await db.Workflows.AnyAsync(
            x => x.ProjectId == workflow.ProjectId &&
                 x.Id != command.Id &&
                 x.Name.ToLower() == command.Name.Trim().ToLower(),
            ct
        );

        if (exists)
        {
            return Result.Fail($"Another workflow with name '{command.Name}' already exists in this project.");
        }

        workflow.Update(command.Name.Trim(), command.Description?.Trim());
        workflow.SetActive(command.IsActive);

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
