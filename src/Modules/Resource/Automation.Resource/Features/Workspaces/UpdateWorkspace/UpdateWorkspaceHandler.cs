using Automation.Resource.Infrastructure.Persistence;
using Automation.Resource.Shared.Dtos;

namespace Automation.Resource.Features.Workspaces.UpdateWorkspace;

internal class UpdateWorkspaceHandler(ResourceDbContext db)
{
    public async Task<Result<WorkspaceDto>> HandleAsync(UpdateWorkspaceCommand command, CancellationToken ct)
    {
        var workspace = await db.Workspaces.FindAsync([command.Id], ct);
        if (workspace is null)
            return Result.Fail($"Workspace with ID '{command.Id}' was not found.");

        workspace.Update(command.Name, command.RootPath);
        await db.SaveChangesAsync(ct);

        return Result.Ok(workspace.Adapt<WorkspaceDto>());
    }
}
