using Automation.Workspace.Features.Workspaces;
using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.WorkspaceAgents.AttachAgentToWorkspace;

public class AttachAgentToWorkspaceEndpoint(IMessageBus bus) : Endpoint<AttachAgentToWorkspaceRequest, WorkspaceAgentDto>
{
    public override void Configure()
    {
        Post("/{workspaceId:guid}/agents");
        Group<WorkspacesGroup>();
        Permissions(P.WorkspaceAgent.Create);
    }

    public override async Task HandleAsync(AttachAgentToWorkspaceRequest req, CancellationToken ct)
    {
        var workspaceId = Route<Guid>("workspaceId");
        var command = new AttachAgentToWorkspaceCommand(workspaceId, req.AgentId, req.RootPath);
        var result = await bus.InvokeAsync<Result<WorkspaceAgentDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}
