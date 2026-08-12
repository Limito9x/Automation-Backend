using Automation.Resource.Infrastructure.Persistence;

namespace Automation.Resource.Features.Workspaces.DeleteWorkspace;

public class DeleteWorkspaceHandler(ResourceDbContext db)
{
    public async Task<Result> HandleAsync(DeleteWorkspaceCommand command, CancellationToken ct)
    {
        var workspace = await db.Workspaces.FindAsync([command.Id], ct);
        if (workspace is null)
            return Result.Fail($"Workspace with ID '{command.Id}' was not found.");

        db.Workspaces.Remove(workspace);
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

