using Automation.Resource.Shared.Dtos;

namespace Automation.Resource.Features.Resources.UploadToRemoteWorkspace;

public class UploadToRemoteWorkspaceEndpoint(IMessageBus bus) : Endpoint<UploadToRemoteWorkspaceCommand, IReadOnlyList<ResourceItemDto>>
{
    public override void Configure()
    {
        Post("/upload-remote");
        Group<ResourcesGroup>();
        Permissions(P.Resource.Create);
    }

    public override async Task HandleAsync(UploadToRemoteWorkspaceCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<IReadOnlyList<ResourceItemDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
