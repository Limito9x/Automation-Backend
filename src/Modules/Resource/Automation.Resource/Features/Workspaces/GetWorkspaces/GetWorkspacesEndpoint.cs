using Automation.Resource.Domain.Enums;
using Automation.Resource.Shared.Dtos;

namespace Automation.Resource.Features.Workspaces.GetWorkspaces;

public class GetWorkspacesEndpoint(IMessageBus bus) : EndpointWithoutRequest<IReadOnlyList<WorkspaceDto>>
{
    public override void Configure()
    {
        Get("/");
        Group<WorkspacesGroup>();
        Permissions(P.Workspace.GetAll);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var projectId = Query<Guid?>("projectId", isRequired: false);
        var kind = Query<WorkspaceKind?>("kind", isRequired: false);
        var agentId = Query<Guid?>("agentId", isRequired: false);

        var result = await bus.InvokeAsync<Result<IReadOnlyList<WorkspaceDto>>>(
            new GetWorkspacesQuery(projectId, kind, agentId), ct);

        await this.SendResultAsync(result, ct);
    }
}
