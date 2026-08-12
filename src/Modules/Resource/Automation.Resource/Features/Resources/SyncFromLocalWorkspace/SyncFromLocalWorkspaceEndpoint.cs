using Automation.Resource.Shared.Dtos;
using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.Resource.Features.Resources.SyncFromLocalWorkspace;

public class SyncFromLocalWorkspaceEndpoint(IMessageBus bus, ICurrentAgent currentAgent) : Endpoint<SyncFromLocalWorkspaceCommand, IReadOnlyList<ResourceItemDto>>
{
    public override void Configure()
    {
        Post("/sync-local");
        Group<ResourcesGroup>();
        AllowAnonymous(); // Auth via X-Agent-Key header
    }

    public override async Task HandleAsync(SyncFromLocalWorkspaceCommand req, CancellationToken ct)
    {
        if (!currentAgent.IsAgentRequest)
        {
            await this.SendResultAsync(Result.Fail("Unauthorized"), ct);
            return;
        }

        var result = await bus.InvokeAsync<Result<IReadOnlyList<ResourceItemDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

