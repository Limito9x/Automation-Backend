using Automation.SharedKernel.Abstractions.Querying;
using Automation.SharedKernel.Infrastructure.Querying;
using Automation.Workspace.Constants;
using Automation.Workspace.Features.Workspaces;
using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.WorkspaceAgents.GetWorkspaceAgentResources;

public class GetWorkspaceAgentResourcesEndpoint(IMessageBus bus) : Endpoint<GetWorkspaceAgentResourcesQuery, PagedResult<WorkspaceAgentResourceDto>>
{
    public override void Configure()
    {
        Get(WorkspaceRoutes.WorkspaceAgentResources);
        Group<WorkspacesGroup>();
        Permissions(P.WorkspaceAgent.GetAll);
    }

    public override async Task HandleAsync(GetWorkspaceAgentResourcesQuery req, CancellationToken ct)
    {
        var workspaceId = Route<Guid>("workspaceId");
        var agentId = Route<Guid>("agentId");
        var projectId = Query<Guid>("projectId");

        req.WorkspaceId = workspaceId;
        req.AgentId = agentId;
        req.ProjectId = projectId;

        var result = await bus.InvokeAsync<Result<PagedResult<WorkspaceAgentResourceDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
