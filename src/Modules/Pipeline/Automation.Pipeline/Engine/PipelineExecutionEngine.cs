using System.Text.Json;
using Automation.Pipeline.Constants;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Engine.Messages;
using Automation.Pipeline.Engine.Models;
using Automation.Pipeline.Infrastructure.Persistence;
using Automation.Pipeline.Tools;
using Automation.Projects.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Automation.Pipeline.Engine;

public class PipelineExecutionEngine(
    PipelineDbContext db,
    IDagPlanner planner,
    IInputResolver inputResolver,
    IAgentBatchBuilder batchBuilder,
    IToolRegistry toolRegistry,
    IExecutionStateStore stateStore,
    IMessageBus messageBus,
    IProjectsApi projectsApi,
    ILogger<PipelineExecutionEngine> logger
) : IPipelineExecutionEngine
{
    public async Task<Result<PipelineExecution>> ExecuteOrResumeAsync(
        Guid executionId,
        Dictionary<string, object?>? runtimeInputs = null,
        CancellationToken ct = default
    )
    {
        var execution = await db.PipelineExecutions
            .Include(x => x.Pipeline)
                .ThenInclude(p => p.Nodes)
            .Include(x => x.Pipeline)
                .ThenInclude(p => p.Edges)
            .Include(x => x.Pipeline)
                .ThenInclude(p => p.Inputs)
            .FirstOrDefaultAsync(x => x.Id == executionId, ct);

        if (execution == null)
        {
            return Result.Fail<PipelineExecution>($"Pipeline execution '{executionId}' not found.");
        }

        if (execution.Status == ExecutionStatus.Succeeded || execution.Status == ExecutionStatus.Cancelled)
        {
            return Result.Ok(execution);
        }

        // Load custom node definitions for this project
        var customDefs = await db.NodeDefinitions
            .AsNoTracking()
            .Where(x => x.ProjectId == execution.Pipeline.ProjectId)
            .ToListAsync(ct);

        // Load existing execution state from Redis (or fallback to DB)
        var state = PipelineExecutionState.FromJsonDocument(execution.ExecutionState);
        if (runtimeInputs != null && runtimeInputs.Count > 0)
        {
            foreach (var (k, v) in runtimeInputs)
            {
                state.RuntimeInputs[k] = v;
                await stateStore.SetStartInputAsync(execution.Id, k, v, ct);
            }
        }
        await stateStore.SaveFullStateAsync(execution.Id, state, ct);

        // Build and validate DAG
        var validation = planner.BuildAndValidateGraph(
            execution.Pipeline,
            customDefs,
            toolRegistry,
            state.RuntimeInputs
        );

        if (!validation.IsValid)
        {
            if (validation.CycleNodeIds.Count > 0)
            {
                var cycleError = $"Pipeline contains cycle involving nodes: {string.Join(", ", validation.CycleNodeIds)}";
                execution.MarkFailed(cycleError, state.ToJsonDocument());
                await db.SaveChangesAsync(ct);
                return Result.Fail<PipelineExecution>(cycleError);
            }

            if (validation.UnresolvedPins.Count > 0)
            {
                return Result.Fail<PipelineExecution>(new UnresolvedPinsError(validation.UnresolvedPins));
            }
        }

        execution.Start();
        await db.SaveChangesAsync(ct);

        var nodes = validation.TopoSortedNodes;

        for (var i = execution.NextNodeIndex; i < nodes.Count; i++)
        {
            var node = nodes[i];
            var isStart = string.Equals(node.Kind, PipelineNodeKind.Start, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(node.RefId, "Start", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(node.RefId, "BeginExecute", StringComparison.OrdinalIgnoreCase);

            var isTool = !isStart && (
                         string.Equals(node.Kind, PipelineNodeKind.Tool, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(node.Kind, "Tool", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(node.Executor, "dotNet", StringComparison.OrdinalIgnoreCase));

            if (isStart)
            {
                // Start node: runtime inputs are already placed in stateStore & state.
                var startOutputs = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

                foreach (var inputDef in execution.Pipeline.Inputs)
                {
                    if (state.RuntimeInputs.TryGetValue(inputDef.Key, out var val))
                    {
                        startOutputs[inputDef.Key] = val;
                    }
                    else
                    {
                        var match = state.RuntimeInputs.FirstOrDefault(x => string.Equals(x.Key, inputDef.Key, StringComparison.OrdinalIgnoreCase));
                        if (match.Key != null && match.Value != null)
                        {
                            startOutputs[inputDef.Key] = match.Value;
                        }
                        else if (inputDef.DefaultValue != null)
                        {
                            startOutputs[inputDef.Key] = inputDef.DefaultValue;
                        }
                    }
                }

                // Also copy any direct runtime inputs
                foreach (var (rKey, rVal) in state.RuntimeInputs)
                {
                    if (!startOutputs.ContainsKey(rKey))
                    {
                        startOutputs[rKey] = rVal;
                    }
                }

                state.SetNodeOutputs(node.NodeId, startOutputs);
                await stateStore.SetNodeOutputsAsync(execution.Id, node.NodeId, startOutputs, ct);
                await stateStore.SetNodeStatusAsync(execution.Id, node.NodeId, ExecutionStatus.Succeeded.ToString(), ct);

                var startDoc = JsonDocument.Parse(JsonSerializer.Serialize(startOutputs));
                var startExecution = new NodeExecution(execution.Id, node.NodeId, startDoc);
                startExecution.MarkSucceeded(startDoc);
                db.NodeExecutions.Add(startExecution);

                execution.SetState(state.ToJsonDocument(), i + 1);
                await db.SaveChangesAsync(ct);
                continue;
            }

            if (isTool)
            {
                var tool = toolRegistry.Get(node.RefId);
                if (tool == null)
                {
                    var err = $"Tool with key '{node.RefId}' not found in ToolRegistry.";
                    execution.MarkFailed(err, state.ToJsonDocument());
                    await db.SaveChangesAsync(ct);
                    return Result.Fail<PipelineExecution>(err);
                }

                logger.LogInformation("Executing .NET Tool [{ToolLabel}] ({ToolKey}) on Node {NodeId}", tool.Label, tool.Key, node.NodeId);

                Dictionary<string, object> toolOutputs;
                try
                {
                    var inputs = inputResolver.ResolveInputs(node, state);
                    logger.LogInformation("Tool [{ToolKey}] Node {NodeId} resolved inputs: {InputsJson}", tool.Key, node.NodeId, JsonSerializer.Serialize(inputs));
                    var toolContext = new ToolExecutionContext(execution.Id, execution.PipelineId, execution.AgentId, ct);
                    toolOutputs = await tool.ExecuteAsync(inputs, toolContext);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error executing Tool [{ToolLabel}] on Node {NodeId}", tool.Label, node.NodeId);
                    execution.MarkFailed($"Error executing Tool '{tool.Label}': {ex.Message}", state.ToJsonDocument());
                    await stateStore.SetNodeStatusAsync(execution.Id, node.NodeId, ExecutionStatus.Failed.ToString(), ct);

                    var failedNodeExecution = new NodeExecution(execution.Id, node.NodeId, null);
                    failedNodeExecution.MarkFailed(ex.Message);
                    db.NodeExecutions.Add(failedNodeExecution);

                    await db.SaveChangesAsync(ct);
                    return Result.Fail<PipelineExecution>(ex.Message);
                }

                var toolOutDict = toolOutputs.ToDictionary(k => k.Key, v => (object?)v.Value);
                state.SetNodeOutputs(node.NodeId, toolOutDict);
                await stateStore.SetNodeOutputsAsync(execution.Id, node.NodeId, toolOutDict, ct);
                await stateStore.SetNodeStatusAsync(execution.Id, node.NodeId, ExecutionStatus.Succeeded.ToString(), ct);

                var progressDoc = JsonDocument.Parse(JsonSerializer.Serialize(toolOutputs));
                var nodeExecution = new NodeExecution(execution.Id, node.NodeId, progressDoc);
                nodeExecution.MarkSucceeded(progressDoc);
                db.NodeExecutions.Add(nodeExecution);

                execution.SetState(state.ToJsonDocument(), i + 1);
                await db.SaveChangesAsync(ct);
            }
            else
            {
                // Group consecutive agent nodes sharing the same executor
                var currentExecutor = node.Executor;
                var batchNodes = new List<DagNode>();
                var batchEndIndex = i;

                while (batchEndIndex < nodes.Count)
                {
                    var candidate = nodes[batchEndIndex];
                    var candidateIsSpecial = string.Equals(candidate.Kind, PipelineNodeKind.Start, StringComparison.OrdinalIgnoreCase) ||
                                             string.Equals(candidate.Kind, PipelineNodeKind.Tool, StringComparison.OrdinalIgnoreCase) ||
                                             string.Equals(candidate.Kind, "Tool", StringComparison.OrdinalIgnoreCase) ||
                                             string.Equals(candidate.Executor, "dotNet", StringComparison.OrdinalIgnoreCase);

                    if (candidateIsSpecial || !string.Equals(candidate.Executor, currentExecutor, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    batchNodes.Add(candidate);
                    batchEndIndex++;
                }

                logger.LogInformation(
                    "Dispatching Agent batch of {Count} steps for executor [{Executor}] (Execution: {ExecutionId}, Agent: {AgentId})",
                    batchNodes.Count,
                    currentExecutor,
                    execution.Id,
                    execution.AgentId
                );

                var batchTask = batchBuilder.BuildBatchTask(execution.Id, batchNodes, state, inputResolver);

                // Fetch Project-Agent executor config (e.g. Unreal project path, settings)
                var configResult = await projectsApi.GetExecutorConfigAsync(
                    execution.Pipeline.ProjectId,
                    execution.AgentId,
                    currentExecutor,
                    ct
                );

                if (configResult.IsSuccess && configResult.Value?.Settings != null)
                {
                    var root = configResult.Value.Settings.RootElement;
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in root.EnumerateObject())
                        {
                            batchTask.EnvironmentConfig[prop.Name] = prop.Value.ValueKind switch
                            {
                                JsonValueKind.String => prop.Value.GetString(),
                                JsonValueKind.Number => prop.Value.GetDouble(),
                                JsonValueKind.True => true,
                                JsonValueKind.False => false,
                                JsonValueKind.Null => null,
                                _ => prop.Value.GetRawText()
                            };
                        }
                    }
                }

                await stateStore.SaveFullStateAsync(execution.Id, state, ct);
                execution.MarkWaitingForAgent(batchTask.StageExecutionId, batchEndIndex, state.ToJsonDocument());
                await db.SaveChangesAsync(ct);

                // Publish task to RabbitMQ / Wolverine queue "stage_tasks.{agent_id}"
                var targetUri = new Uri($"rabbitmq://queue/stage_tasks.{execution.AgentId}");
                await messageBus.EndpointFor(targetUri).SendAsync(batchTask);

                return Result.Ok(execution);
            }
        }

        // All nodes completed
        execution.MarkSucceeded(state.ToJsonDocument());
        await stateStore.SaveFullStateAsync(execution.Id, state, ct);
        await db.SaveChangesAsync(ct);
        await stateStore.ExpireExecutionAsync(execution.Id, TimeSpan.FromHours(48), ct);
        logger.LogInformation("Pipeline execution {ExecutionId} succeeded", execution.Id);

        return Result.Ok(execution);
    }
}
