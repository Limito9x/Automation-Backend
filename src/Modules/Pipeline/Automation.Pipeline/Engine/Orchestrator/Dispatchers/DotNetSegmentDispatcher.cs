using System.Text.Json;
using Automation.Pipeline.Constants;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Engine.DataResolver;
using Automation.Pipeline.Engine.Models;
using Automation.Pipeline.Infrastructure.Persistence;
using Automation.Pipeline.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Automation.Pipeline.Engine.Orchestrator.Dispatchers;

public class DotNetSegmentDispatcher(
    PipelineDbContext db,
    IToolRegistry toolRegistry,
    IPinValueResolver pinResolver,
    IExecutionMemoryStore memoryStore,
    ILogger<DotNetSegmentDispatcher> logger
)
{
    public async Task<Result> DispatchAsync(
        PipelineExecution execution,
        ExecSegment segment,
        ScopeContext? scope = null,
        CancellationToken ct = default
    )
    {
        foreach (var step in segment.Steps)
        {
            var isStart = string.Equals(step.Kind, PipelineNodeKind.Start, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(step.RefId, "Start", StringComparison.OrdinalIgnoreCase);

            if (isStart)
            {
                await RecordNodeSuccessAsync(execution.Id, step.NodeId, new Dictionary<string, object>(), ct);
                continue;
            }

            var tool = toolRegistry.Get(step.RefId);
            if (tool == null)
            {
                var err = $"DotNet Tool '{step.RefId}' not found for step '{step.Label}'.";
                logger.LogError(err);
                return Result.Fail(err);
            }

            // 1. Pull all inputs on-demand
            var resolvedInputs = await pinResolver.ResolveAllPinsAsync(
                execution.Id,
                step.NodeId,
                scope: scope,
                ct: ct
            );

            // Cast to Dictionary<string, object>
            var toolInputs = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var (k, v) in resolvedInputs)
            {
                if (v != null) toolInputs[k] = v;
            }

            // 2. Execute Tool
            var toolContext = new ToolExecutionContext(execution.Id, execution.PipelineId, execution.AgentId, ct, step.NodeId);
            Dictionary<string, object> outputs;
            try
            {
                logger.LogInformation("Executing DotNet Tool [{ToolLabel}] ({NodeId})", step.Label, step.NodeId);
                outputs = await tool.ExecuteAsync(toolInputs, toolContext);
            }
            catch (Exception ex)
            {
                var err = $"Execution of tool '{step.Label}' failed: {ex.Message}";
                logger.LogError(ex, err);
                await RecordNodeFailureAsync(execution.Id, step.NodeId, err, ct);
                return Result.Fail(err);
            }

            // 3. Save outputs to memory store for downstream pull
            var outputsDict = outputs.ToDictionary(k => k.Key, v => (object?)v.Value);
            await memoryStore.SetNodeAllOutputsAsync(execution.Id, step.NodeId, outputsDict, scope, ct);

            // 4. Record success in DB
            await RecordNodeSuccessAsync(execution.Id, step.NodeId, outputs, ct);
        }

        return Result.Ok();
    }

    private async Task RecordNodeSuccessAsync(Guid executionId, Guid nodeId, Dictionary<string, object> outputs, CancellationToken ct)
    {
        var nodeExec = await db.NodeExecutions
            .FirstOrDefaultAsync(x => x.PipelineExecutionId == executionId && x.PipelineNodeId == nodeId, ct);

        var outputJson = JsonSerializer.Serialize(outputs);
        var outputDoc = JsonDocument.Parse(outputJson);

        if (nodeExec == null)
        {
            nodeExec = new NodeExecution(executionId, nodeId, status: ExecutionStatus.Running);
            nodeExec.MarkSucceeded(outputDoc);
            db.NodeExecutions.Add(nodeExec);
        }
        else
        {
            nodeExec.MarkSucceeded(outputDoc);
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task RecordNodeFailureAsync(Guid executionId, Guid nodeId, string error, CancellationToken ct)
    {
        var nodeExec = await db.NodeExecutions
            .FirstOrDefaultAsync(x => x.PipelineExecutionId == executionId && x.PipelineNodeId == nodeId, ct);

        if (nodeExec == null)
        {
            nodeExec = new NodeExecution(executionId, nodeId, status: ExecutionStatus.Running);
            nodeExec.MarkFailed(error);
            db.NodeExecutions.Add(nodeExec);
        }
        else
        {
            nodeExec.MarkFailed(error);
        }

        await db.SaveChangesAsync(ct);
    }
}
