using Automation.Resource.Shared.Dtos;

namespace Automation.Resource.Features.Workspaces.CreateWorkspace;

public class CreateWorkspaceEndpoint(IMessageBus bus) : Endpoint<CreateWorkspaceCommand, WorkspaceDto>
{
    public override void Configure()
    {
        Post("/");
        Group<WorkspacesGroup>();
        Permissions(P.Workspace.Create);
    }

    public override async Task HandleAsync(CreateWorkspaceCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<WorkspaceDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

