using System.Text.Json;
using Automation.Pipeline.Constants;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Automation.Pipeline.Tools;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

using Automation.Pipeline.Engine.StructRegistry;

namespace Automation.Pipeline.Features.Pipelines.GetPipelineGraph;

[NonTransactional]
public class GetPipelineGraphHandler(
    PipelineDbContext db,
    IToolRegistry toolRegistry,
    IEntityStructRegistry structRegistry
)
{
    public async Task<Result<PipelineGraphDto>> HandleAsync(
        GetPipelineGraphQuery query,
        CancellationToken ct
    )
    {
        var pipeline = await db.Pipelines
            .AsNoTracking()
            .Include(x => x.Nodes)
            .Include(x => x.Edges)
            .Include(x => x.Inputs)
            .FirstOrDefaultAsync(x => x.Id == query.PipelineId, ct);

        if (pipeline == null)
        {
            return Result.Fail<PipelineGraphDto>($"Pipeline '{query.PipelineId}' not found.");
        }

        var customDefs = await db.NodeDefinitions
            .AsNoTracking()
            .Where(x => x.ProjectId == pipeline.ProjectId)
            .ToListAsync(ct);

        var nodeDtos = new List<PipelineNodeGraphDto>();

        foreach (var node in pipeline.Nodes)
        {
            var isStartNode = string.Equals(node.Kind, PipelineNodeKind.Start, StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(node.RefId, "Start", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(node.RefId, "BeginExecute", StringComparison.OrdinalIgnoreCase);

            IReadOnlyList<PinDefinition> inputs = [];
            IReadOnlyList<PinDefinition> outputs = [];
            string label = node.RefId;
            string? category = null;
            string? executor = null;

            Dictionary<string, object?>? configValues = null;
            if (node.Config != null)
            {
                try
                {
                    configValues = JsonSerializer.Deserialize<Dictionary<string, object?>>(node.Config);
                }
                catch
                {
                    // Fallback to empty if not a dictionary
                }
            }

            if (isStartNode)
            {
                label = "Start";
                category = "System";
                executor = "builtin";
                var startOutputs = pipeline.Inputs.OrderBy(i => i.Order).Select(i => new PinDefinition
                {
                    Id = i.Key,
                    Label = i.Label,
                    Kind = PinKind.Data,
                    PrimitiveType = i.Type,
                    Cardinality = i.Cardinality,
                    IsRequired = i.IsRequired,
                    DefaultValue = i.DefaultValue
                }).ToList();
                var (pInputs, pOutputs) = FlowPinHelper.WithExecPins(PipelineNodeKind.Start, isPure: false, [], startOutputs);
                inputs = pInputs;
                outputs = pOutputs;
            }
            // 1. Check in ToolRegistry first (for all BuiltIn tools)
            else if (toolRegistry.Get(node.RefId) is { } tool)
            {
                var toolInputs = tool.Inputs;
                var toolOutputs = tool.Outputs;

                if (string.Equals(tool.Key, "BreakStruct", StringComparison.OrdinalIgnoreCase))
                {
                    var structType = configValues?.GetValueOrDefault("StructType")?.ToString() ?? "Resource";
                    if (structRegistry.Get(structType) is { } sDef)
                    {
                        toolOutputs = sDef.OutputPins;
                    }
                }
                else if (string.Equals(tool.Key, "AppendString", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(tool.Key, "MakeArray", StringComparison.OrdinalIgnoreCase))
                {
                    if (configValues?.TryGetValue("DynamicPins", out var dpObj) == true && dpObj is JsonElement dpElem && dpElem.ValueKind == JsonValueKind.Array)
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
                            toolInputs = dynamicList;
                        }
                    }
                }

                var (pInputs, pOutputs) = FlowPinHelper.WithExecPins(PipelineNodeKind.Tool, tool.IsPure, toolInputs, toolOutputs);
                inputs = pInputs;
                outputs = pOutputs;
                label = !string.IsNullOrWhiteSpace(tool.Label) ? tool.Label : tool.Key;
                category = CategorizeTool(tool.Key);
                executor = "builtin";
            }
            else
            {
                // 2. Check in Project NodeDefinitions
                var def = customDefs.FirstOrDefault(x =>
                    string.Equals(x.Key, node.RefId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Id.ToString(), node.RefId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Name, node.RefId, StringComparison.OrdinalIgnoreCase)
                );

                if (def != null)
                {
                    var (pInputs, pOutputs) = FlowPinHelper.WithExecPins(def);
                    inputs = pInputs;
                    outputs = pOutputs;
                    label = !string.IsNullOrWhiteSpace(def.Label) ? def.Label : def.Name;
                    category = "Custom";
                    executor = def.Executor;
                }
            }

            nodeDtos.Add(new PipelineNodeGraphDto(
                node.Id,
                node.RefId,
                isStartNode ? PipelineNodeKind.Start : node.Kind,
                label,
                category,
                executor,
                node.Position,
                inputs,
                outputs,
                configValues
            ));
        }

        var edgeDtos = pipeline.Edges.Select(e => new PipelineEdgeGraphDto(
            e.Id,
            e.SourcePipelineNodeId,
            e.SourcePin,
            e.TargetPipelineNodeId,
            e.TargetPin,
            e.Kind
        )).ToList();

        var inputDtos = pipeline.Inputs.OrderBy(i => i.Order).Select(i => new PipelineInputDto(
            i.Id,
            i.Key,
            i.Label,
            i.Type,
            i.Cardinality,
            i.IsRequired,
            i.DefaultValue,
            i.Order
        )).ToList();

        var graphDto = new PipelineGraphDto(
            pipeline.Id,
            pipeline.ProjectId,
            pipeline.Name,
            nodeDtos,
            edgeDtos,
            inputDtos
        );

        return Result.Ok(graphDto);
    }

    private static string CategorizeTool(string key) =>
        key switch
        {
            "BreakStruct" => "Data / Struct",
            "GetTagValueFromInspection" => "Inspection & Tag",
            "SyncLocalChangeToWorkspace" => "Workspace",
            "MakeArray" or "AppendString" or "CombinePath" or "StaticValue" => "Utility",
            _ => "Tools"
        };
}
