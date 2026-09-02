using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Workflows.DeleteWorkflow;

[Transactional(typeof(PipelineDbContext))]
public class DeleteWorkflowHandler(PipelineDbContext db)
{
    public async Task<Result> HandleAsync(
        DeleteWorkflowCommand command,
        CancellationToken ct
    )
    {
        var workflow = await db.Workflows.FirstOrDefaultAsync(x => x.Id == command.Id, ct);

        if (workflow == null)
        {
            return Result.Fail($"Workflow with ID '{command.Id}' not found.");
        }

        db.Workflows.Remove(workflow);
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
