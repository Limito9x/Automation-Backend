using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.Workspaces.CreateWorkspace;

public class CreateWorkspaceHandler(WorkspaceDbContext db)
{
    public async Task<Result<WorkspaceDto>> HandleAsync(CreateWorkspaceCommand command, CancellationToken ct)
    {
        var workspace = new Domain.Entities.Workspace(
            command.ProjectId,
            command.Name
        );

        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync(ct);

        return Result.Ok(workspace.Adapt<WorkspaceDto>());
    }
}

