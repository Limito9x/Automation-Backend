using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.Workspaces.UpdateWorkspace;

public class UpdateWorkspaceHandler(WorkspaceDbContext db)
{
    public async Task<Result<WorkspaceDto>> HandleAsync(UpdateWorkspaceCommand command, CancellationToken ct)
    {
        var workspace = await db.Workspaces.FindAsync([command.Id], ct);
        if (workspace is null)
            return Result.Fail($"Workspace with ID '{command.Id}' was not found.");

        workspace.Update(command.Name);
        await db.SaveChangesAsync(ct);

        return Result.Ok(workspace.Adapt<WorkspaceDto>());
    }
}

