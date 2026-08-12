using Automation.Resource.Shared.Dtos;

namespace Automation.Resource.Features.Resources.GetResources;

public class GetResourcesEndpoint(IMessageBus bus) : EndpointWithoutRequest<IReadOnlyList<ResourceItemDto>>
{
    public override void Configure()
    {
        Get("/");
        Group<ResourcesGroup>();
        Permissions(P.Resource.GetAll);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var projectId = Query<Guid?>("projectId", isRequired: false);
        var workspaceId = Query<Guid?>("workspaceId", isRequired: false);
        var platformExtensionId = Query<Guid?>("platformExtensionId", isRequired: false);
        var contentId = Query<Guid?>("contentId", isRequired: false);

        var result = await bus.InvokeAsync<Result<IReadOnlyList<ResourceItemDto>>>(
            new GetResourcesQuery(projectId, workspaceId, platformExtensionId, contentId), ct);

        await this.SendResultAsync(result, ct);
    }
}
