using System.Text.Json;
using Automation.Pipeline.Constants;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Features.Pipelines.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Automation.Pipeline.Tools;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

using Automation.Pipeline.Engine.StructRegistry;

namespace Automation.Pipeline.Features.Pipelines.AddPipelineNode;

[Transactional(typeof(PipelineDbContext))]
public class AddPipelineNodeHandler(
    PipelineDbContext db,
    IToolRegistry toolRegistry,
    IEntityStructRegistry structRegistry
)
{
    public async Task<Result<PipelineNodeGraphDto>> HandleAsync(
        AddPipelineNodeCommand command,
        CancellationToken ct
    )
    {
        var pipeline = await db.Pipelines
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.PipelineId, ct);

        if (pipeline == null)
        {
            return Result.Fail<PipelineNodeGraphDto>($"Pipeline '{command.PipelineId}' not found.");
        }

        JsonDocument? configDoc = null;
        if (command.ConfigValues != null && command.ConfigValues.Count > 0)
        {
            configDoc = JsonDocument.Parse(JsonSerializer.Serialize(command.ConfigValues));
        }

        var node = new PipelineNode(
            Guid.NewGuid(),
            command.PipelineId,
            command.RefId,
            command.Kind,
            command.PositionX,
            command.PositionY,
            configDoc
        );

        await db.PipelineNodes.AddAsync(node, ct);
        await db.SaveChangesAsync(ct);

        // Resolve definitions for DTO
        IReadOnlyList<PinDefinition> inputs = [];
        IReadOnlyList<PinDefinition> outputs = [];
        string label = node.RefId;
        string? category = null;
        string? executor = null;

        // 1. Check in ToolRegistry first (for all BuiltIn tools)
        var tool = toolRegistry.Get(node.RefId);
        if (tool != null)
        {
            var toolInputs = tool.Inputs;
            var toolOutputs = tool.Outputs;

            if (string.Equals(tool.Key, "BreakStruct", StringComparison.OrdinalIgnoreCase))
            {
                var structType = command.ConfigValues?.GetValueOrDefault("StructType")?.ToString() ?? "Resource";
                if (structRegistry.Get(structType) is { } sDef)
                {
                    toolOutputs = sDef.OutputPins;
                }
            }
            else if (string.Equals(tool.Key, "AppendString", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(tool.Key, "MakeArray", StringComparison.OrdinalIgnoreCase))
            {
                if (command.ConfigValues?.TryGetValue("DynamicPins", out var dpObj) == true && dpObj != null)
                {
                    var pinNames = dpObj is IEnumerable<string> strEnum ? strEnum :
                                   dpObj is IEnumerable<object> objEnum ? objEnum.Select(x => x.ToString()!) :
                                   dpObj is JsonElement jsonEl && jsonEl.ValueKind == JsonValueKind.Array
                                       ? jsonEl.EnumerateArray().Select(x => x.GetString()!).Where(x => !string.IsNullOrEmpty(x))
                                       : [];

                    var dynamicList = pinNames.Select(pinId => new PinDefinition
                    {
                        Id = pinId,
                        Label = pinId.StartsWith("Item_") ? pinId.Replace("_", " ") : pinId,
                        PrimitiveType = PinPrimitiveType.String,
                        Cardinality = PinCardinality.Single,
                        IsRequired = false
                    }).ToList();

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
            var def = await db.NodeDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.ProjectId == pipeline.ProjectId &&
                    (x.Key == node.RefId || x.Id.ToString() == node.RefId || x.Name == node.RefId),
                    ct
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

        var nodeDto = new PipelineNodeGraphDto(
            node.Id,
            node.RefId,
            node.Kind,
            label,
            category,
            executor,
            node.Position,
            inputs,
            outputs,
            command.ConfigValues
        );

        return Result.Ok(nodeDto);
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
