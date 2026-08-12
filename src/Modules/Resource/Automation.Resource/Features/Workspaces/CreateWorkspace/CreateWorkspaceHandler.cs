using Automation.Resource.Domain.Enums;
using Automation.Resource.Infrastructure.Persistence;
using Automation.Resource.Shared.Dtos;

namespace Automation.Resource.Features.Workspaces.CreateWorkspace;

public class CreateWorkspaceHandler(ResourceDbContext db)
{
    public async Task<Result<WorkspaceDto>> HandleAsync(CreateWorkspaceCommand command, CancellationToken ct)
    {
        var workspace = new Domain.Entities.Workspace(
            command.ProjectId,
            command.PlatformId,
            command.Name,
            command.Kind,
            command.RootPath,
            command.AgentId
        );

        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync(ct);

        return Result.Ok(workspace.Adapt<WorkspaceDto>());
    }
}

