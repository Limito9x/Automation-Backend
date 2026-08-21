using System.Text.Json;
using Automation.Pipeline.Constants;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Engine.Models;
using Automation.Pipeline.Engine.StructRegistry;
using Automation.Pipeline.Tools;

namespace Automation.Pipeline.Engine;

public class DagPlanner(IEntityStructRegistry? structRegistry = null) : IDagPlanner
{
    public GraphValidationResult BuildAndValidateGraph(
        Domain.Entities.Pipeline pipeline,
        IReadOnlyList<NodeDefinition> customDefinitions,
        IToolRegistry toolRegistry,
        Dictionary<string, object?>? runtimeInputs = null
    )
    {
        var customDefsLookup = customDefinitions
            .GroupBy(x => x.Id.ToString())
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var customDefsKeyLookup = customDefinitions
            .Where(x => !string.IsNullOrEmpty(x.Key))
            .GroupBy(x => x.Key)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var dagNodes = new Dictionary<Guid, DagNode>();

        foreach (var node in pipeline.Nodes)
        {
            var isStartNode = string.Equals(node.Kind, PipelineNodeKind.Start, StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(node.RefId, "Start", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(node.RefId, "BeginExecute", StringComparison.OrdinalIgnoreCase);

            IReadOnlyList<PinDefinition> inputs = [];
            IReadOnlyList<PinDefinition> outputs = [];
            var label = node.RefId;
            var executor = "dotNet";

            if (isStartNode)
            {
                label = "Start";
                executor = "dotNet";
                inputs = [];
                outputs = pipeline.Inputs.OrderBy(i => i.Order).Select(i => new PinDefinition
                {
                    Id = i.Key,
                    Label = i.Label,
                    Kind = PinKind.Data,
                    PrimitiveType = i.Type,
                    Cardinality = i.Cardinality,
                    IsRequired = i.IsRequired,
                    DefaultValue = i.DefaultValue
                }).ToList();
            }
            else if (toolRegistry.Get(node.RefId) is { } tool)
            {
                inputs = tool.Inputs;
                outputs = tool.Outputs;

                if (string.Equals(tool.Key, "BreakStruct", StringComparison.OrdinalIgnoreCase) && structRegistry != null && node.Config != null)
                {
                    var structType = GetConfigString(node.Config, "StructType") ?? "Resource";
                    if (structRegistry.Get(structType) is { } sDef)
                    {
                        outputs = sDef.OutputPins;
                    }
                }
                else if ((string.Equals(tool.Key, "AppendString", StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(tool.Key, "MakeArray", StringComparison.OrdinalIgnoreCase)) && node.Config != null)
                {
                    if (node.Config.RootElement.TryGetProperty("DynamicPins", out var dpElem) && dpElem.ValueKind == JsonValueKind.Array)
                    {
                        var dynamicList = new List<PinDefinition>();
                        foreach (var item in dpElem.EnumerateArray())
                        {
                            var pinId = item.GetString();
                            if (!string.IsNullOrEmpty(pinId))
                            {
                                dynamicList.Add(new PinDefinition
                                {
                                    Id = pinId,
                                    Label = pinId.StartsWith("Item_") ? pinId.Replace("_", " ") : pinId,
                                    PrimitiveType = PinPrimitiveType.String,
                                    Cardinality = PinCardinality.Single,
                                    IsRequired = false
                                });
                            }
                        }
                        if (dynamicList.Count > 0)
                        {
                            inputs = dynamicList;
                        }
                    }
                }

                label = !string.IsNullOrWhiteSpace(tool.Label) ? tool.Label : tool.Key;
                executor = "dotNet";
            }
            else
            {
                NodeDefinition? def = null;
                if (customDefsLookup.TryGetValue(node.RefId, out var foundDef))
                {
                    def = foundDef;
                }
                else if (customDefsKeyLookup.TryGetValue(node.RefId, out foundDef))
                {
                    def = foundDef;
                }

                if (def != null)
                {
                    inputs = def.Inputs;
                    outputs = def.Outputs;
                    label = !string.IsNullOrEmpty(def.Label) ? def.Label : def.Name;
                    executor = !string.IsNullOrEmpty(def.Executor) ? def.Executor : "blender";
                }
            }

            var incoming = pipeline.Edges
                .Where(e => e.TargetPipelineNodeId == node.Id)
                .Select(e => new IncomingPinConnection(e.TargetPin, e.SourcePipelineNodeId, e.SourcePin))
                .ToList();

            var nodeKind = isStartNode ? PipelineNodeKind.Start :
                           toolRegistry.Get(node.RefId) != null ? PipelineNodeKind.Tool :
                           node.Kind;

            dagNodes[node.Id] = new DagNode
            {
                NodeId = node.Id,
                RefId = node.RefId,
                Kind = nodeKind,
                Label = label,
                Executor = executor,
                InputPins = inputs,
                OutputPins = outputs,
                IncomingConnections = incoming,
                Config = node.Config
            };
        }

        // Execution Ordering: Exec Chain DFS (when Exec edges exist) or Fallback to Kahn
        var execEdges = pipeline.Edges
            .Where(e => string.Equals(e.SourcePin, "exec_out", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(e.TargetPin, "exec_in", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var topoSortedNodes = new List<DagNode>();
        var cycleNodeIds = new List<string>();

        if (execEdges.Count > 0)
        {
            // --- EXEC CHAIN DFS (UE5 Control Flow Engine) ---
            var visited = new HashSet<Guid>();
            var execAdjacency = execEdges
                .GroupBy(e => e.SourcePipelineNodeId)
                .ToDictionary(g => g.Key, g => g.First().TargetPipelineNodeId);

            var execTargets = execEdges.Select(e => e.TargetPipelineNodeId).ToHashSet();

            // Find Entry Point: Node with Start kind, BeginExecute, OR node having exec_out but no incoming exec_in
            var entryNode = pipeline.Nodes.FirstOrDefault(n => string.Equals(n.Kind, PipelineNodeKind.Start, StringComparison.OrdinalIgnoreCase))
                            ?? pipeline.Nodes.FirstOrDefault(n => string.Equals(n.RefId, "BeginExecute", StringComparison.OrdinalIgnoreCase))
                            ?? pipeline.Nodes.FirstOrDefault(n => execAdjacency.ContainsKey(n.Id) && !execTargets.Contains(n.Id))
                            ?? pipeline.Nodes.FirstOrDefault(n => execAdjacency.ContainsKey(n.Id));

            if (entryNode != null)
            {
                // Add Entry/Start node first
                if (dagNodes.TryGetValue(entryNode.Id, out var startDagNode) && visited.Add(entryNode.Id))
                {
                    topoSortedNodes.Add(startDagNode);
                }

                // Helper to check if a node is pure (in-memory C# calculation)
                bool IsPureNode(Guid nodeId) =>
                    dagNodes.TryGetValue(nodeId, out var n) &&
                    toolRegistry.Get(n.RefId) is { IsPure: true };

                // Helper to check if a pure node depends on an unexecuted Action node output
                bool HasDependencyOnAction(Guid nodeId, HashSet<Guid> visiting)
                {
                    visiting.Add(nodeId);
                    var dataIncoming = pipeline.Edges
                        .Where(e => e.TargetPipelineNodeId == nodeId && !string.Equals(e.TargetPin, "exec_in", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var edge in dataIncoming)
                    {
                        var srcNode = dagNodes.GetValueOrDefault(edge.SourcePipelineNodeId);
                        if (srcNode != null && !IsPureNode(edge.SourcePipelineNodeId) && !string.Equals(srcNode.Kind, PipelineNodeKind.Start, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        if (!visiting.Contains(edge.SourcePipelineNodeId) && HasDependencyOnAction(edge.SourcePipelineNodeId, visiting))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                // Pre-resolve independent Pure Data nodes upfront so continuous Action steps fuse into 1 Stage
                void ResolvePureDataNode(Guid nodeId)
                {
                    var dataIncoming = pipeline.Edges
                        .Where(e => e.TargetPipelineNodeId == nodeId && !string.Equals(e.TargetPin, "exec_in", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var edge in dataIncoming)
                    {
                        if (!visited.Contains(edge.SourcePipelineNodeId) && IsPureNode(edge.SourcePipelineNodeId))
                        {
                            ResolvePureDataNode(edge.SourcePipelineNodeId);
                        }
                    }

                    if (dagNodes.TryGetValue(nodeId, out var pNode) && visited.Add(nodeId))
                    {
                        topoSortedNodes.Add(pNode);
                    }
                }

                foreach (var (nId, _) in dagNodes)
                {
                    if (IsPureNode(nId) && !visited.Contains(nId) && !HasDependencyOnAction(nId, []))
                    {
                        ResolvePureDataNode(nId);
                    }
                }

                // Helper to resolve remaining data dependency nodes
                void ResolveDataDependencies(Guid nodeId)
                {
                    var dataIncoming = pipeline.Edges
                        .Where(e => e.TargetPipelineNodeId == nodeId && !string.Equals(e.TargetPin, "exec_in", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var edge in dataIncoming)
                    {
                        if (!visited.Contains(edge.SourcePipelineNodeId))
                        {
                            ResolveDataDependencies(edge.SourcePipelineNodeId);
                            if (dagNodes.TryGetValue(edge.SourcePipelineNodeId, out var depNode) && visited.Add(depNode.NodeId))
                            {
                                topoSortedNodes.Add(depNode);
                            }
                        }
                    }
                }

                var currentId = execAdjacency.TryGetValue(entryNode.Id, out var firstActionId) ? (Guid?)firstActionId : null;
                while (currentId.HasValue)
                {
                    var cId = currentId.Value;
                    if (visited.Contains(cId))
                    {
                        cycleNodeIds.Add(cId.ToString());
                        break;
                    }

                    // 1. Resolve any remaining data dependencies
                    ResolveDataDependencies(cId);

                    // 2. Add the action node itself
                    if (dagNodes.TryGetValue(cId, out var actionNode) && visited.Add(cId))
                    {
                        topoSortedNodes.Add(actionNode);
                    }

                    // 3. Follow exec_out to next node
                    currentId = execAdjacency.TryGetValue(cId, out var nextId) ? nextId : null;
                }
            }
        }
        else
        {
            // --- FALLBACK: Kahn's Algorithm for pure data graphs ---
            var inDegree = new Dictionary<Guid, int>();
            var adjacency = new Dictionary<Guid, List<Guid>>();

            foreach (var node in pipeline.Nodes)
            {
                inDegree[node.Id] = 0;
                adjacency[node.Id] = [];
            }

            foreach (var edge in pipeline.Edges)
            {
                if (dagNodes.ContainsKey(edge.SourcePipelineNodeId) && dagNodes.ContainsKey(edge.TargetPipelineNodeId))
                {
                    inDegree[edge.TargetPipelineNodeId]++;
                    adjacency[edge.SourcePipelineNodeId].Add(edge.TargetPipelineNodeId);
                }
            }

            var queue = new Queue<Guid>();
            foreach (var (nodeId, deg) in inDegree)
            {
                if (deg == 0)
                {
                    queue.Enqueue(nodeId);
                }
            }

            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                if (dagNodes.TryGetValue(u, out var dagNode))
                {
                    topoSortedNodes.Add(dagNode);
                }

                foreach (var v in adjacency[u])
                {
                    inDegree[v]--;
                    if (inDegree[v] == 0)
                    {
                        queue.Enqueue(v);
                    }
                }
            }

            if (topoSortedNodes.Count < pipeline.Nodes.Count)
            {
                var visitedIds = topoSortedNodes.Select(x => x.NodeId).ToHashSet();
                cycleNodeIds = pipeline.Nodes
                    .Where(n => !visitedIds.Contains(n.Id))
                    .Select(n => n.Id.ToString())
                    .ToList();
            }
        }

        // Pin Pre-flight Validation
        var unresolvedPins = new List<UnresolvedPin>();

        foreach (var node in topoSortedNodes)
        {
            foreach (var pin in node.InputPins)
            {
                // Skip exec pins from data validation
                if (pin.Kind == PinKind.Exec || string.Equals(pin.Id, "exec_in", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var isConnected = node.IncomingConnections.Any(c =>
                    string.Equals(c.TargetPinKey, pin.Id, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.TargetPinKey, pin.Label, StringComparison.OrdinalIgnoreCase));

                var hasInlineValue = HasConfigValue(node.Config, pin.Id) || HasConfigValue(node.Config, pin.Label);
                var hasDefaultValue = pin.DefaultValue != null;
                var hasRuntimeInput = runtimeInputs != null &&
                    (runtimeInputs.ContainsKey(pin.Id) ||
                     runtimeInputs.ContainsKey(pin.Label) ||
                     runtimeInputs.ContainsKey($"{node.NodeId}:{pin.Id}"));

                if (!isConnected && !hasInlineValue && !hasDefaultValue && !hasRuntimeInput && pin.IsRequired)
                {
                    unresolvedPins.Add(new UnresolvedPin(
                        node.NodeId,
                        node.Label,
                        pin.Id,
                        pin.Label,
                        pin.PrimitiveType,
                        "Required input pin is not connected and has no inline or default value."
                    ));
                }
            }
        }

        return new GraphValidationResult
        {
            CycleNodeIds = cycleNodeIds,
            UnresolvedPins = unresolvedPins,
            TopoSortedNodes = topoSortedNodes
        };
    }

    private static bool HasConfigValue(JsonDocument? config, string key)
    {
        if (config == null || string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        if (config.RootElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in config.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, key, StringComparison.OrdinalIgnoreCase))
                {
                    return prop.Value.ValueKind switch
                    {
                        JsonValueKind.Null or JsonValueKind.Undefined => false,
                        JsonValueKind.String => !string.IsNullOrEmpty(prop.Value.GetString()),
                        JsonValueKind.Array => prop.Value.GetArrayLength() > 0,
                        _ => true
                    };
                }
            }
        }

        return false;
    }

    private static string? GetConfigString(JsonDocument? config, string key)
    {
        if (config == null || string.IsNullOrWhiteSpace(key))
            return null;

        if (config.RootElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in config.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, key, StringComparison.OrdinalIgnoreCase))
                {
                    return prop.Value.ValueKind == JsonValueKind.String
                        ? prop.Value.GetString()
                        : prop.Value.GetRawText().Trim('"');
                }
            }
        }

        return null;
    }
}
