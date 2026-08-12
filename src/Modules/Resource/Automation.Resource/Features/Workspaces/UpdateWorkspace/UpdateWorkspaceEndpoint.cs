using Automation.Resource.Shared.Dtos;

namespace Automation.Resource.Features.Workspaces.UpdateWorkspace;

public class UpdateWorkspaceEndpoint(IMessageBus bus) : Endpoint<UpdateWorkspaceRequest, WorkspaceDto>
{
    public override void Configure()
    {
        Put("/{id:guid}");
        Group<WorkspacesGroup>();
        Permissions(P.Workspace.Update);
    }

    public override async Task HandleAsync(UpdateWorkspaceRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result<WorkspaceDto>>(new UpdateWorkspaceCommand(id, req.Name, req.RootPath), ct);
        await this.SendResultAsync(result, ct);
    }
}
