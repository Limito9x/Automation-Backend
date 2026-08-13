using Automation.Workspace.Shared.Dtos;

namespace Automation.Workspace.Features.Workspaces.GetWorkspaces;

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

        var result = await bus.InvokeAsync<Result<IReadOnlyList<WorkspaceDto>>>(
            new GetWorkspacesQuery(projectId), ct);

        await this.SendResultAsync(result, ct);
    }
}

