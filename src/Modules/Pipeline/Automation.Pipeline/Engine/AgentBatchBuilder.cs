using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Engine.Messages;
using Automation.Pipeline.Engine.Models;

namespace Automation.Pipeline.Engine;

public class AgentBatchBuilder : IAgentBatchBuilder
{
    public StageTaskMessage BuildBatchTask(
        Guid pipelineExecutionId,
        IReadOnlyList<DagNode> batchNodes,
        PipelineExecutionState state,
        IInputResolver inputResolver
    )
    {
        if (batchNodes.Count == 0)
        {
            throw new ArgumentException("Batch nodes must not be empty", nameof(batchNodes));
        }

        var executor = batchNodes[0].Executor;
        var batchNodeIds = batchNodes.Select(n => n.NodeId).ToHashSet();
        var stageExecutionId = Guid.NewGuid().ToString();

        var resolvedData = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        // Preload resolved data from execution state
        foreach (var (nId, outputs) in state.NodeOutputs)
        {
            foreach (var (pinKey, val) in outputs)
            {
                resolvedData[$"{nId}_{pinKey}"] = val;
            }
        }
        foreach (var (rKey, val) in state.RuntimeInputs)
        {
            resolvedData[rKey] = val;
        }

        var steps = new List<StepExecution>();

        for (var i = 0; i < batchNodes.Count; i++)
        {
            var node = batchNodes[i];
            var stepId = node.NodeId.ToString();
            var stepInputs = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            var stepParams = new List<StepExecutionParam>();
            var inputMappings = new List<StepInputMapping>();

            // Resolve direct inputs for this node (from state or inline config)
            var directInputs = inputResolver.ResolveInputs(node, state);

            foreach (var pin in node.InputPins)
            {
                if (pin.Kind == PinKind.Exec || string.Equals(pin.Id, "exec_in", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var pinKey = !string.IsNullOrEmpty(pin.Id) ? pin.Id : pin.Label;

                // 1. Check incoming connection from graph
                var connection = node.IncomingConnections.FirstOrDefault(c =>
                    string.Equals(c.TargetPinKey, pin.Id, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.TargetPinKey, pin.Label, StringComparison.OrdinalIgnoreCase));

                if (connection != null)
                {
                    inputMappings.Add(new StepInputMapping
                    {
                        PinKey = pinKey,
                        SourceKind = "node_output",
                        SourceNodeId = connection.SourceNodeId.ToString(),
                        SourcePinKey = connection.SourcePinKey
                    });

                    // Intra-batch reference support
                    if (batchNodeIds.Contains(connection.SourceNodeId))
                    {
                        stepInputs[pinKey] = new Dictionary<string, string>
                        {
                            { "$ref", $"{connection.SourceNodeId}.{connection.SourcePinKey}" }
                        };
                    }
                    else if (directInputs.TryGetValue(pinKey, out var directVal))
                    {
                        stepInputs[pinKey] = directVal;
                    }
                }
                else if (directInputs.TryGetValue(pinKey, out var directVal))
                {
                    inputMappings.Add(new StepInputMapping
                    {
                        PinKey = pinKey,
                        SourceKind = "literal",
                        LiteralValue = directVal
                    });

                    stepInputs[pinKey] = directVal;

                    if (directVal != null)
                    {
                        var strVal = directVal is string s ? s : JsonSerializer.Serialize(directVal);
                        stepParams.Add(new StepExecutionParam { Key = pinKey, Value = strVal });
                    }
                }
            }

            var stepOutputs = node.OutputPins
                .Where(p => p.Kind != PinKind.Exec && !string.Equals(p.Id, "exec_out", StringComparison.OrdinalIgnoreCase))
                .Select(p => !string.IsNullOrEmpty(p.Id) ? p.Id : p.Label)
                .ToList();

            steps.Add(new StepExecution
            {
                StepExecutionId = stepId,
                StepType = node.RefId,
                Name = node.Label,
                ScriptPath = node.RefId,
                Order = i + 1,
                InputMappings = inputMappings,
                Inputs = stepInputs,
                Outputs = stepOutputs,
                Params = stepParams
            });
        }

        return new StageTaskMessage
        {
            StageExecutionId = stageExecutionId,
            PipelineExecutionId = pipelineExecutionId.ToString(),
            StageId = batchNodes[0].NodeId.ToString(),
            Executor = executor,
            AccessToken = stageExecutionId,
            GrpcEndpoint = "http://127.0.0.1:50051",
            Steps = steps,
            ResolvedData = resolvedData
        };
    }
}
