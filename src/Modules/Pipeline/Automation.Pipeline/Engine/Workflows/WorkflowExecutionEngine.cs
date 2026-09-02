using System.Net.Http.Json;
using System.Text.Json;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Features.Pipelines.RunPipeline;
using Automation.Pipeline.Hubs;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Automation.Pipeline.Engine.Workflows;

public class WorkflowExecutionEngine(
    PipelineDbContext db,
    IMessageBus messageBus,
    IHubContext<WorkflowExecutionHub> hubContext,
    IHttpClientFactory httpClientFactory,
    ILogger<WorkflowExecutionEngine> logger
) : IWorkflowExecutionEngine
{
    public async Task<WorkflowExecution> ExecuteAsync(
        Workflow workflow,
        WorkflowEventContext context,
        CancellationToken ct = default
    )
    {
        var execution = new WorkflowExecution(workflow.Id, context.EventType, context.RawPayload);
        execution.Start();
        db.WorkflowExecutions.Add(execution);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Started Workflow Execution {ExecutionId} for Workflow {WorkflowId} ({WorkflowName})",
            execution.Id, workflow.Id, workflow.Name);

        await BroadcastWorkflowStartedAsync(workflow.Id, execution, ct);

        try
        {
            var nodes = workflow.Nodes.ToList();
            var edges = workflow.Edges.ToList();

            var startNodes = nodes.Where(n => n.Kind == WorkflowNodeKind.EventTrigger).ToList();
            if (startNodes.Count == 0)
            {
                // Fallback to nodes that have no incoming edges
                var targetNodeIds = edges.Select(e => e.TargetWorkflowNodeId).ToHashSet();
                startNodes = nodes.Where(n => !targetNodeIds.Contains(n.Id)).ToList();
            }

            if (startNodes.Count == 0)
            {
                throw new InvalidOperationException("No entry point / EventTrigger node found in workflow.");
            }

            var visitedNodes = new HashSet<Guid>();
            var queue = new Queue<WorkflowNode>(startNodes);

            while (queue.Count > 0)
            {
                var currentNode = queue.Dequeue();
                if (visitedNodes.Contains(currentNode.Id))
                {
                    continue;
                }
                visitedNodes.Add(currentNode.Id);

                var nodeExec = new WorkflowNodeExecution(execution.Id, currentNode.Id);
                nodeExec.MarkRunning();
                db.WorkflowNodeExecutions.Add(nodeExec);
                await db.SaveChangesAsync(ct);

                await BroadcastNodeUpdatedAsync(workflow.Id, nodeExec, ct);

                string? activeBranchPin = null;

                try
                {
                    activeBranchPin = await ExecuteNodeLogicAsync(currentNode, context, nodeExec, ct);
                    if (nodeExec.Status == ExecutionStatus.Running)
                    {
                        nodeExec.MarkSucceeded();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error executing WorkflowNode {NodeId} ({Kind}) in Execution {ExecutionId}",
                        currentNode.Id, currentNode.Kind, execution.Id);
                    nodeExec.MarkFailed(ex.Message);
                    await db.SaveChangesAsync(ct);
                    await BroadcastNodeUpdatedAsync(workflow.Id, nodeExec, ct);
                    throw;
                }

                await db.SaveChangesAsync(ct);
                await BroadcastNodeUpdatedAsync(workflow.Id, nodeExec, ct);

                // Find next nodes along outgoing edges
                var outgoingEdges = edges.Where(e => e.SourceWorkflowNodeId == currentNode.Id).ToList();
                if (!string.IsNullOrEmpty(activeBranchPin))
                {
                    // Filter by specific branch pin (e.g. true_out or false_out)
                    outgoingEdges = outgoingEdges.Where(e =>
                        string.Equals(e.SourcePin, activeBranchPin, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(e.SourcePin, "exec_out", StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                foreach (var edge in outgoingEdges)
                {
                    var nextNode = nodes.FirstOrDefault(n => n.Id == edge.TargetWorkflowNodeId);
                    if (nextNode != null && !visitedNodes.Contains(nextNode.Id))
                    {
                        queue.Enqueue(nextNode);
                    }
                }
            }

            execution.MarkSucceeded();
            await db.SaveChangesAsync(ct);
            await BroadcastWorkflowFinishedAsync(workflow.Id, execution, ct);
        }
        catch (Exception ex)
        {
            execution.MarkFailed(ex.Message);
            await db.SaveChangesAsync(ct);
            await BroadcastWorkflowFinishedAsync(workflow.Id, execution, ct);
        }

        return execution;
    }

    private async Task<string?> ExecuteNodeLogicAsync(
        WorkflowNode node,
        WorkflowEventContext context,
        WorkflowNodeExecution nodeExec,
        CancellationToken ct
    )
    {
        switch (node.Kind)
        {
            case WorkflowNodeKind.EventTrigger:
                // Event trigger passes context through
                nodeExec.MarkSucceeded(JsonSerializer.SerializeToDocument(new
                {
                    context.EventType,
                    context.ProjectId,
                    context.WorkspaceId,
                    context.ResourceVersionIds,
                    context.Extension,
                    context.RelativePath
                }));
                return "exec_out";

            case WorkflowNodeKind.ConditionFilter:
                return EvaluateConditionFilter(node, context, nodeExec);

            case WorkflowNodeKind.ExecutePipeline:
                return await ExecutePipelineNodeAsync(node, context, nodeExec, ct);

            case WorkflowNodeKind.SendNotification:
                return await ExecuteSendNotificationNodeAsync(node, context, nodeExec, ct);

            default:
                nodeExec.MarkSucceeded();
                return "exec_out";
        }
    }

    private string EvaluateConditionFilter(
        WorkflowNode node,
        WorkflowEventContext context,
        WorkflowNodeExecution nodeExec
    )
    {
        bool isMatch = true;

        if (node.Config != null)
        {
            var root = node.Config.RootElement;

            // 1. Extension Filter
            if (root.TryGetProperty("extensions", out var extElem) && extElem.ValueKind == JsonValueKind.Array)
            {
                var allowedExts = extElem.EnumerateArray()
                    .Select(e => e.GetString()?.Trim().ToLowerInvariant())
                    .Where(e => !string.IsNullOrEmpty(e))
                    .ToHashSet();

                if (allowedExts.Count > 0)
                {
                    var currentExt = context.Extension?.Trim().ToLowerInvariant();
                    if (string.IsNullOrEmpty(currentExt) || !allowedExts.Contains(currentExt))
                    {
                        isMatch = false;
                    }
                }
            }

            // 2. WorkspaceId Filter
            if (isMatch && root.TryGetProperty("workspaceId", out var wsElem))
            {
                if (wsElem.TryGetGuid(out var filterWsId) && filterWsId != Guid.Empty)
                {
                    if (context.WorkspaceId != filterWsId)
                    {
                        isMatch = false;
                    }
                }
            }

            // 3. Path Pattern Filter (contains or wildcard)
            if (isMatch && root.TryGetProperty("pathPattern", out var pathElem))
            {
                var pattern = pathElem.GetString();
                if (!string.IsNullOrWhiteSpace(pattern) && !string.IsNullOrWhiteSpace(context.RelativePath))
                {
                    if (!context.RelativePath.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        isMatch = false;
                    }
                }
            }
        }

        nodeExec.MarkSucceeded(JsonSerializer.SerializeToDocument(new { Match = isMatch }));
        return isMatch ? "true_out" : "false_out";
    }

    private async Task<string?> ExecutePipelineNodeAsync(
        WorkflowNode node,
        WorkflowEventContext context,
        WorkflowNodeExecution nodeExec,
        CancellationToken ct
    )
    {
        if (node.Config == null)
        {
            throw new InvalidOperationException($"Node {node.Id} is missing configuration for ExecutePipeline.");
        }

        var root = node.Config.RootElement;
        if (!root.TryGetProperty("pipelineId", out var pElem) || !pElem.TryGetGuid(out var pipelineId))
        {
            throw new InvalidOperationException($"Node {node.Id} has no valid PipelineId configured.");
        }

        Guid agentId = context.AgentId;
        if (root.TryGetProperty("agentId", out var aElem) && aElem.TryGetGuid(out var cfgAgentId) && cfgAgentId != Guid.Empty)
        {
            agentId = cfgAgentId;
        }

        var runtimeInputs = new Dictionary<string, object?>();

        // Map inputs from bindings if provided
        if (root.TryGetProperty("inputBindings", out var bindingsElem) && bindingsElem.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in bindingsElem.EnumerateObject())
            {
                var inputKey = prop.Name;
                var sourceMapping = prop.Value.GetString();

                if (string.Equals(sourceMapping, "ResourceVersionId", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(sourceMapping, "ResourceId", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(sourceMapping, "Target", StringComparison.OrdinalIgnoreCase))
                {
                    runtimeInputs[inputKey] = context.ResourceVersionIds.FirstOrDefault();
                }
                else if (string.Equals(sourceMapping, "WorkspaceId", StringComparison.OrdinalIgnoreCase))
                {
                    runtimeInputs[inputKey] = context.WorkspaceId;
                }
                else if (string.Equals(sourceMapping, "RelativePath", StringComparison.OrdinalIgnoreCase))
                {
                    runtimeInputs[inputKey] = context.RelativePath;
                }
                else
                {
                    runtimeInputs[inputKey] = sourceMapping;
                }
            }
        }
        else
        {
            // Default automatic mapping: If pipeline has inputs, map ResourceVersionId to "Target" or "Resource"
            if (context.ResourceVersionIds.Count > 0)
            {
                runtimeInputs["Target"] = context.ResourceVersionIds.First();
                runtimeInputs["ResourceVersionId"] = context.ResourceVersionIds.First();
            }
            runtimeInputs["WorkspaceId"] = context.WorkspaceId;
        }

        var command = new RunPipelineCommand(
            PipelineId: pipelineId,
            AgentId: agentId,
            RuntimeInputs: runtimeInputs
        );

        var result = await messageBus.InvokeAsync<Result<PipelineExecutionDto>>(command, ct);

        if (result.IsFailed)
        {
            var err = string.Join("; ", result.Errors.Select(e => e.Message));
            nodeExec.MarkFailed(err);
            throw new InvalidOperationException($"Pipeline execution failed: {err}");
        }

        nodeExec.MarkSucceeded(JsonSerializer.SerializeToDocument(new
        {
            PipelineExecutionId = result.Value.Id,
            Status = result.Value.Status.ToString()
        }));

        return "exec_out";
    }

    private async Task<string?> ExecuteSendNotificationNodeAsync(
        WorkflowNode node,
        WorkflowEventContext context,
        WorkflowNodeExecution nodeExec,
        CancellationToken ct
    )
    {
        if (node.Config != null)
        {
            var root = node.Config.RootElement;
            if (root.TryGetProperty("webhookUrl", out var urlElem))
            {
                var url = urlElem.GetString();
                if (!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    var client = httpClientFactory.CreateClient();
                    var payload = new
                    {
                        EventType = context.EventType.ToString(),
                        context.ProjectId,
                        context.WorkspaceId,
                        context.RelativePath,
                        Timestamp = DateTimeOffset.UtcNow
                    };

                    try
                    {
                        var response = await client.PostAsJsonAsync(uri, payload, ct);
                        nodeExec.MarkSucceeded(JsonSerializer.SerializeToDocument(new
                        {
                            StatusCode = (int)response.StatusCode,
                            IsSuccess = response.IsSuccessStatusCode
                        }));
                        return "exec_out";
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to send webhook to {Url}", url);
                        nodeExec.MarkFailed($"Webhook error: {ex.Message}");
                        return "exec_out";
                    }
                }
            }
        }

        nodeExec.MarkSucceeded();
        return "exec_out";
    }

    private async Task BroadcastWorkflowStartedAsync(Guid workflowId, WorkflowExecution execution, CancellationToken ct)
    {
        try
        {
            await hubContext.Clients.Group($"workflow_{workflowId}").SendAsync("WorkflowExecutionStarted", new
            {
                ExecutionId = execution.Id,
                WorkflowId = workflowId,
                TriggerEventType = execution.TriggerEventType.ToString(),
                Status = execution.Status.ToString(),
                StartedAt = execution.StartedAt
            }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SignalR broadcast failed for WorkflowExecutionStarted");
        }
    }

    private async Task BroadcastNodeUpdatedAsync(Guid workflowId, WorkflowNodeExecution nodeExec, CancellationToken ct)
    {
        try
        {
            await hubContext.Clients.Group($"workflow_{workflowId}").SendAsync("WorkflowNodeExecutionUpdated", new
            {
                ExecutionId = nodeExec.WorkflowExecutionId,
                WorkflowNodeId = nodeExec.WorkflowNodeId,
                Status = nodeExec.Status.ToString(),
                StartedAt = nodeExec.StartedAt,
                FinishedAt = nodeExec.FinishedAt,
                Output = nodeExec.Output,
                ErrorMessage = nodeExec.ErrorMessage
            }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SignalR broadcast failed for WorkflowNodeExecutionUpdated");
        }
    }

    private async Task BroadcastWorkflowFinishedAsync(Guid workflowId, WorkflowExecution execution, CancellationToken ct)
    {
        try
        {
            await hubContext.Clients.Group($"workflow_{workflowId}").SendAsync("WorkflowExecutionFinished", new
            {
                ExecutionId = execution.Id,
                WorkflowId = workflowId,
                Status = execution.Status.ToString(),
                FinishedAt = execution.FinishedAt,
                ErrorMessage = execution.ErrorMessage
            }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SignalR broadcast failed for WorkflowExecutionFinished");
        }
    }
}
