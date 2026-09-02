using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Engine.Workflows;
using Automation.Pipeline.Infrastructure.Persistence;
using Automation.Workspace.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Workflows.EventBridge;

[NonTransactional]
public class ResourcesCreatedWorkflowBridgeHandler(
    PipelineDbContext db,
    IWorkflowExecutionEngine engine,
    ILogger<ResourcesCreatedWorkflowBridgeHandler> logger
)
{
    public async Task HandleAsync(ResourcesCreatedEvent evt, CancellationToken ct)
    {
        logger.LogInformation("ResourcesCreatedWorkflowBridgeHandler received event for Workspace {WorkspaceId}, Project {ProjectId} with {Count} resources",
            evt.WorkspaceId, evt.ProjectId, evt.ResourceVersions.Count);

        var activeWorkflows = await db.Workflows
            .Where(w => w.ProjectId == evt.ProjectId && w.IsActive)
            .Include(w => w.Nodes)
            .Include(w => w.Edges)
            .ToListAsync(ct);

        if (activeWorkflows.Count == 0)
        {
            logger.LogDebug("No active workflows found for Project {ProjectId}", evt.ProjectId);
            return;
        }

        foreach (var workflow in activeWorkflows)
        {
            var triggerNode = workflow.Nodes.FirstOrDefault(n => n.Kind == WorkflowNodeKind.EventTrigger);
            if (triggerNode == null)
            {
                continue;
            }

            if (!MatchesTriggerConfig(triggerNode, evt))
            {
                continue;
            }

            var context = new WorkflowEventContext
            {
                EventType = WorkflowEventType.OnResourceCreated,
                ProjectId = evt.ProjectId,
                WorkspaceId = evt.WorkspaceId,
                AgentId = evt.AgentId,
                ResourceVersionIds = evt.ResourceVersionIds,
                RawPayload = JsonSerializer.SerializeToDocument(evt)
            };

            try
            {
                await engine.ExecuteAsync(workflow, context, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error triggering Workflow {WorkflowId} from ResourcesCreatedEvent", workflow.Id);
            }
        }
    }

    private static bool MatchesTriggerConfig(Domain.Entities.WorkflowNode triggerNode, ResourcesCreatedEvent evt)
    {
        if (triggerNode.Config == null)
        {
            return true;
        }

        var root = triggerNode.Config.RootElement;

        // 1. Check EventType
        if (root.TryGetProperty("eventType", out var etElem))
        {
            var etStr = etElem.GetString();
            if (!string.IsNullOrEmpty(etStr) && !string.Equals(etStr, nameof(WorkflowEventType.OnResourceCreated), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // 2. Check WorkspaceId
        if (root.TryGetProperty("workspaceId", out var wsElem) && wsElem.TryGetGuid(out var wsId) && wsId != Guid.Empty)
        {
            if (evt.WorkspaceId != wsId)
            {
                return false;
            }
        }

        return true;
    }
}
