using Automation.SharedKernel.Abstractions.Querying;

namespace Automation.Workspace.Features.WorkspaceAgents.GetWorkspaceAgentResources;

public class GetWorkspaceAgentResourcesQuery : PagedQuery
{
    public Guid WorkspaceId { get; set; }
    public Guid AgentId { get; set; }
    public Guid ProjectId { get; set; }
}
