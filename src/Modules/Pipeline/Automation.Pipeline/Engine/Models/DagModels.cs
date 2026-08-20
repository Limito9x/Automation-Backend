using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Engine.Models;

public record IncomingPinConnection(string TargetPinKey, Guid SourceNodeId, string SourcePinKey);

public class DagNode
{
    public Guid NodeId { get; init; }
    public string RefId { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Executor { get; init; } = "dotNet";
    public IReadOnlyList<PinDefinition> InputPins { get; init; } = [];
    public IReadOnlyList<PinDefinition> OutputPins { get; init; } = [];
    public List<IncomingPinConnection> IncomingConnections { get; init; } = [];
    public JsonDocument? Config { get; init; }
}

public record UnresolvedPin(
    Guid NodeId,
    string NodeLabel,
    string PinKey,
    string PinLabel,
    PinPrimitiveType PrimitiveType,
    string Reason
);

public class GraphValidationResult
{
    public bool IsValid => CycleNodeIds.Count == 0 && UnresolvedPins.Count == 0;
    public List<string> CycleNodeIds { get; init; } = [];
    public List<UnresolvedPin> UnresolvedPins { get; init; } = [];
    public List<DagNode> TopoSortedNodes { get; init; } = [];
}

public class PipelineExecutionState
{
    public Dictionary<string, Dictionary<string, object?>> NodeOutputs { get; set; } = new();
    public Dictionary<string, object?> RuntimeInputs { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();

    public object? GetNodeOutput(Guid nodeId, string pinKey)
    {
        var key = nodeId.ToString();
        // 1. Try exact or case-insensitive node key
        var nodeEntry = NodeOutputs.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
        if (nodeEntry.Value != null)
        {
            var outputs = nodeEntry.Value;
            if (outputs.TryGetValue(pinKey, out var value) && value != null)
            {
                return value;
            }
            var match = outputs.FirstOrDefault(x => string.Equals(x.Key, pinKey, StringComparison.OrdinalIgnoreCase));
            if (match.Key != null && match.Value != null)
            {
                return match.Value;
            }
            // If only 1 output in source node, return it
            if (outputs.Count == 1 && outputs.Values.FirstOrDefault() != null)
            {
                return outputs.Values.FirstOrDefault();
            }
        }
        return null;
    }

    public void SetNodeOutput(Guid nodeId, string pinKey, object? value)
    {
        var key = nodeId.ToString();
        if (!NodeOutputs.TryGetValue(key, out var outputs))
        {
            outputs = new Dictionary<string, object?>();
            NodeOutputs[key] = outputs;
        }
        outputs[pinKey] = value;
    }

    public void SetNodeOutputs(Guid nodeId, Dictionary<string, object?> outputs)
    {
        var key = nodeId.ToString();
        if (!NodeOutputs.TryGetValue(key, out var existingOutputs))
        {
            NodeOutputs[key] = new Dictionary<string, object?>(outputs);
        }
        else
        {
            foreach (var (k, v) in outputs)
            {
                existingOutputs[k] = v;
            }
        }
    }

    public JsonDocument ToJsonDocument()
    {
        var json = JsonSerializer.Serialize(this);
        return JsonDocument.Parse(json);
    }

    public static PipelineExecutionState FromJsonDocument(JsonDocument? doc)
    {
        if (doc == null)
        {
            return new PipelineExecutionState();
        }
        try
        {
            var raw = doc.RootElement.GetRawText();
            return JsonSerializer.Deserialize<PipelineExecutionState>(raw) ?? new PipelineExecutionState();
        }
        catch
        {
            return new PipelineExecutionState();
        }
    }
}
