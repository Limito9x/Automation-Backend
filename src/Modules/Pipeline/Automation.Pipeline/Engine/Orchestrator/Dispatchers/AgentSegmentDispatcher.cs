using System.Text.Json;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Engine.DataResolver;
using Automation.Pipeline.Engine.Messages;
using Automation.Pipeline.Engine.Models;
using Automation.Projects.Contracts;
using Microsoft.Extensions.Logging;

namespace Automation.Pipeline.Engine.Orchestrator.Dispatchers;

public class AgentSegmentDispatcher(
    IMessageBus messageBus,
    IProjectsApi projectsApi,
    IPinValueResolver pinResolver,
    ILogger<AgentSegmentDispatcher> logger
)
{
    public async Task<Result> DispatchAsync(
        PipelineExecution execution,
        ExecSegment segment,
        int nextSegmentIndex,
        IReadOnlyList<NodeDefinition> customDefinitions,
        ScopeContext? scope = null,
        CancellationToken ct = default
    )
    {
        var stageId = $"stage_{Guid.NewGuid():N}";
        var customDefsLookup = customDefinitions
            .GroupBy(x => x.Id.ToString())
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var customDefsKeyLookup = customDefinitions
            .Where(x => !string.IsNullOrEmpty(x.Key))
            .GroupBy(x => x.Key)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var steps = new List<StepExecution>();
        for (var i = 0; i < segment.Steps.Count; i++)
        {
            var step = segment.Steps[i];
            NodeDefinition? def = null;
            if (customDefsLookup.TryGetValue(step.RefId, out var foundDef) ||
                customDefsKeyLookup.TryGetValue(step.RefId, out foundDef))
            {
                def = foundDef;
            }

            var inputMappings = step.IncomingConnections.Select(c => new StepInputMapping
            {
                PinKey = c.TargetPinKey,
                SourceKind = "node_output",
                SourceNodeId = c.SourceNodeId.ToString(),
                SourcePinKey = c.SourcePinKey
            }).ToList();

            // 1. Resolve inputs on Backend via PinValueResolver (start inputs, pure nodes, configs, assets, prior steps)
            var resolvedPins = await pinResolver.ResolveAllPinsAsync(
                execution.Id,
                step.NodeId,
                scope: scope,
                ct: ct
            );

            var stepInputs = new Dictionary<string, object?>(resolvedPins);

            // 2. For intra-segment wires (nodes within the same segment), mark as $ref so stage_runner uses Blender RAM
            foreach (var conn in step.IncomingConnections)
            {
                if (segment.Steps.Any(s => s.NodeId == conn.SourceNodeId))
                {
                    stepInputs[conn.TargetPinKey] = new Dictionary<string, string>
                    {
                        ["$ref"] = $"{conn.SourceNodeId}.{conn.SourcePinKey}"
                    };
                }
            }

            steps.Add(new StepExecution
            {
                StepExecutionId = step.NodeId.ToString(),
                StepType = step.Kind,
                Name = step.Label,
                ScriptPath = def != null && !string.IsNullOrEmpty(def.Key) ? def.Key : step.RefId,
                Order = i,
                InputMappings = inputMappings,
                Inputs = stepInputs
            });

            logger.LogInformation("AgentSegmentDispatcher: Step #{Order} ({Name}) [{StepId}] resolved inputs: {InputsJson}",
                i, step.Label, step.NodeId, JsonSerializer.Serialize(stepInputs));
        }

        // Fetch Environment Config for Executor (e.g. blender executable path, unreal engine port)
        var envConfig = new Dictionary<string, object?>();
        var configResult = await projectsApi.GetExecutorConfigAsync(
            execution.Pipeline.ProjectId,
            execution.AgentId,
            segment.Executor,
            ct
        );

        if (configResult != null && configResult.IsSuccess && configResult.Value?.Settings != null)
        {
            try
            {
                envConfig = JsonSerializer.Deserialize<Dictionary<string, object?>>(configResult.Value.Settings.RootElement.GetRawText()) ?? [];
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to parse executor settings for {Executor}", segment.Executor);
            }
        }

        var stageTask = new StageTaskMessage
        {
            StageExecutionId = stageId,
            PipelineExecutionId = execution.Id.ToString(),
            StageId = stageId,
            Executor = segment.Executor,
            Steps = steps,
            ResolvedData = [], // Worker pulls via gRPC on demand
            EnvironmentConfig = envConfig
        };

        execution.MarkWaitingForAgent(stageId, nextSegmentIndex, execution.ExecutionState ?? JsonDocument.Parse("{}"));

        logger.LogInformation("Dispatching Agent Segment [{StageId}] with {StepCount} steps to {Executor}",
            stageId, steps.Count, segment.Executor);

        if (execution.AgentId != Guid.Empty)
        {
            var queueName = $"stage_tasks.{execution.AgentId}";
            logger.LogInformation("Routing StageTaskMessage to targeted agent queue: {QueueName}", queueName);
            var endpointUri = new Uri($"rabbitmq://queue/{queueName}");
            var endpoint = messageBus.EndpointFor(endpointUri);
            await endpoint.SendAsync(stageTask);
        }
        else
        {
            await messageBus.PublishAsync(stageTask);
        }

        return Result.Ok();
    }
}
