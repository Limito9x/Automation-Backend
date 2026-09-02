using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Automation.Workspace.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Pipelines.EventBridge;

[NonTransactional]
public class ResourcesCreatedPipelineBridgeHandler(
    PipelineDbContext db,
    IMessageBus bus,
    ILogger<ResourcesCreatedPipelineBridgeHandler> logger
)
{
    public async Task Handle(ResourcesCreatedEvent message, CancellationToken ct)
    {
        var targetPipelines = await db.Pipelines
            .AsNoTracking()
            .Where(x => x.ProjectId == message.ProjectId &&
                        x.TriggerType == PipelineTriggerType.OnResourceCreated &&
                        (x.TriggerWorkspaceId == null || x.TriggerWorkspaceId == message.WorkspaceId))
            .ToListAsync(ct);

        if (targetPipelines.Count == 0)
        {
            return;
        }

        logger.LogInformation(
            "Found {Count} event-triggered pipeline(s) matching OnResourceCreated for Project '{ProjectId}', Workspace '{WorkspaceId}'.",
            targetPipelines.Count,
            message.ProjectId,
            message.WorkspaceId
        );

        foreach (var pipeline in targetPipelines)
        {
            foreach (var rv in message.ResourceVersions)
            {
                var runtimeInputs = new Dictionary<string, object?>
                {
                    ["Resource"] = $"resource:{rv.ResourceVersionId}",
                    ["Workspace"] = $"workspace:{message.WorkspaceId}"
                };

                logger.LogInformation(
                    "Auto-triggering Pipeline '{PipelineName}' ({PipelineId}) for ResourceVersion '{ResourceVersionId}'.",
                    pipeline.Name,
                    pipeline.Id,
                    rv.ResourceVersionId
                );

                await bus.InvokeAsync<Result<PipelineExecutionDto>>(
                    new RunPipelineCommand(pipeline.Id, message.AgentId, runtimeInputs),
                    ct
                );
            }
        }
    }
}
