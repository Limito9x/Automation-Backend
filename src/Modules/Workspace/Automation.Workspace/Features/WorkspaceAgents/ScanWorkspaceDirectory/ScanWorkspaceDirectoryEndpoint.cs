using Automation.Workspace.Constants;
using Automation.Workspace.Features.Workspaces;
using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.WorkspaceAgents.ScanWorkspaceDirectory;

public class ScanWorkspaceDirectoryEndpoint(IMessageBus bus) : Endpoint<ScanWorkspaceDirectoryRequest, IReadOnlyList<DirectoryNodeDto>>
{
    public override void Configure()
    {
        Post(WorkspaceRoutes.ScanDirectory);
        Group<WorkspacesGroup>();
        Permissions(P.WorkspaceAgent.GetAll);
    }

    public override async Task HandleAsync(ScanWorkspaceDirectoryRequest req, CancellationToken ct)
    {
        var workspaceId = Route<Guid>("workspaceId");
        var agentId = Route<Guid>("agentId");

        var query = new ScanWorkspaceDirectoryQuery(workspaceId, agentId, req.RelativePath);
        var result = await bus.InvokeAsync<Result<IReadOnlyList<DirectoryNodeDto>>>(query, ct);

        await this.SendResultAsync(result, ct);
    }
}
