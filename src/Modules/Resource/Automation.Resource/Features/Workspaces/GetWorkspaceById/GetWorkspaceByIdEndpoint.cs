using Automation.Resource.Shared.Dtos;

namespace Automation.Resource.Features.Workspaces.GetWorkspaceById;

public class GetWorkspaceByIdEndpoint(IMessageBus bus) : EndpointWithoutRequest<WorkspaceDto>
{
    public override void Configure()
    {
        Get("/{id:guid}");
        Group<WorkspacesGroup>();
        Permissions(P.Workspace.GetById);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result<WorkspaceDto>>(new GetWorkspaceByIdQuery(id), ct);
        await this.SendResultAsync(result, ct);
    }
}

