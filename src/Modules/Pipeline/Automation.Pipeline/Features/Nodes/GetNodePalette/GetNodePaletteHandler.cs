using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Infrastructure.Persistence;
using Automation.Pipeline.Tools;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Nodes.GetNodePalette;

[NonTransactional]
public class GetNodePaletteHandler(IToolRegistry toolRegistry, PipelineDbContext db)
{
    public async Task<Result<IReadOnlyList<NodePaletteItemDto>>> HandleAsync(
        GetNodePaletteQuery query,
        CancellationToken ct
    )
    {
        var result = new List<NodePaletteItemDto>();

        // 1. Built-in Tools from ToolRegistry
        var builtInTools = toolRegistry.GetAll();
        foreach (var tool in builtInTools)
        {
            if (string.Equals(tool.Key, "Start", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tool.Key, "BeginExecute", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var (pInputs, pOutputs) = FlowPinHelper.WithExecPins(tool);
            var category = CategorizeTool(tool.Key);
            result.Add(new NodePaletteItemDto(
                tool.Key,
                tool.Label,
                category,
                "BuiltIn",
                "builtin",
                pInputs,
                pOutputs,
                null
            ));
        }

        // 2. Custom User NodeDefinitions from DB
        var customNodesQuery = db.NodeDefinitions.AsNoTracking();
        if (query.ProjectId.HasValue)
        {
            customNodesQuery = customNodesQuery.Where(x => x.ProjectId == query.ProjectId.Value);
        }

        var customNodes = await customNodesQuery
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        foreach (var node in customNodes)
        {
            var (pInputs, pOutputs) = FlowPinHelper.WithExecPins(node);
            result.Add(new NodePaletteItemDto(
                node.Key,
                node.Label,
                "Custom",
                "Custom",
                node.Executor,
                pInputs,
                pOutputs,
                node.Id
            ));
        }

        return Result.Ok<IReadOnlyList<NodePaletteItemDto>>(result);
    }

    private static string CategorizeTool(string key) =>
        key switch
        {
            "BreakStruct" => "Data / Struct",
            "GetResourceInspection" or "GetTagValueFromInspection" => "Inspection & Tag",
            "SyncLocalChangeToWorkspace" => "Workspace",
            "MakeArray" or "AppendString" or "CombinePath" or "StaticValue" => "Utility",
            _ => "Tools"
        };
}
