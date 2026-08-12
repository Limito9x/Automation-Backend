namespace Automation.Resource.Features.Workspaces.DeleteWorkspace;

public class DeleteWorkspaceEndpoint(IMessageBus bus) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/{id:guid}");
        Group<WorkspacesGroup>();
        Permissions(P.Workspace.Delete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result>(new DeleteWorkspaceCommand(id), ct);
        await this.SendResultAsync(result, ct);
    }
}

