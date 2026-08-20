using System.Collections;
using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Workspace.Contracts;

namespace Automation.Pipeline.Tools.Workspaces;

public class SyncLocalChangeToWorkspaceTool(IWorkspaceApi workspaceApi) : IResolverTool
{
    public string Key => "SyncLocalChangeToWorkspace";
    public string Label => "Sync Local Change To Workspace";

    public IReadOnlyList<PinDefinition> Inputs =>
        new List<PinDefinition>
        {
            new PinDefinition
            {
                Id = "WorkspaceId",
                Label = "Target Workspace",
                PrimitiveType = PinPrimitiveType.EntityRef,
                Cardinality = PinCardinality.Single,
                IsRequired = true,
                Metadata = """{"type": "entity-select", "properties": {"entity": "Workspace"}}""",
            },
            new PinDefinition
            {
                Id = "RelativePaths",
                Label = "Relative Paths",
                PrimitiveType = PinPrimitiveType.Path,
                Cardinality = PinCardinality.Array,
                IsRequired = true,
            },
            new PinDefinition
            {
                Id = "Notes",
                Label = "Notes",
                PrimitiveType = PinPrimitiveType.String,
                Cardinality = PinCardinality.Single,
                IsRequired = false,
                DefaultValue = "Sync from Pipeline",
            }
        };

    public IReadOnlyList<PinDefinition> Outputs =>
        new List<PinDefinition>
        {
            new PinDefinition
            {
                Id = "AddedCount",
                Label = "Added Count",
                PrimitiveType = PinPrimitiveType.Number,
                Cardinality = PinCardinality.Single,
                IsRequired = true,
            },
            new PinDefinition
            {
                Id = "ModifiedCount",
                Label = "Modified Count",
                PrimitiveType = PinPrimitiveType.Number,
                Cardinality = PinCardinality.Single,
                IsRequired = true,
            },
            new PinDefinition
            {
                Id = "LocationRemoved",
                Label = "Location Removed",
                PrimitiveType = PinPrimitiveType.Number,
                Cardinality = PinCardinality.Single,
                IsRequired = true,
            },
        };

    public async Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    )
    {
        var workspaceId = inputs.TryGetValue("WorkspaceId", out var wVal) && wVal is Guid wGuid
            ? wGuid
            : Guid.TryParse(inputs.GetValueOrDefault("WorkspaceId")?.ToString(), out var wParsed)
                ? wParsed
                : (Guid?)null;

        if (workspaceId == null)
            throw new ArgumentException("WorkspaceId is required.");

        var agentId = context.AgentId;
        if (agentId == Guid.Empty)
            throw new ArgumentException("AgentId in execution context cannot be empty.");

        var targetPaths = new List<string>();
        if (inputs.TryGetValue("RelativePaths", out var rawPaths) && rawPaths != null)
        {
            if (rawPaths is IEnumerable<string> strEnumerable)
            {
                targetPaths.AddRange(strEnumerable.Where(x => !string.IsNullOrWhiteSpace(x)));
            }
            else if (rawPaths is string strSingle)
            {
                if (!string.IsNullOrWhiteSpace(strSingle))
                    targetPaths.Add(strSingle);
            }
            else if (rawPaths is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonElement.EnumerateArray())
                {
                    var val = item.GetString();
                    if (!string.IsNullOrWhiteSpace(val))
                        targetPaths.Add(val);
                }
            }
            else if (rawPaths is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    var str = item?.ToString();
                    if (!string.IsNullOrWhiteSpace(str))
                        targetPaths.Add(str);
                }
            }
        }

        var notes = inputs.TryGetValue("Notes", out var nObj) && nObj != null
            ? nObj.ToString()
            : "Sync from Pipeline";

        var result = await workspaceApi.SyncLocalChangesAsync(
            workspaceId.Value,
            agentId,
            targetPaths,
            notes,
            context.CancellationToken
        );

        if (result.IsFailed)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Message)));

        return new Dictionary<string, object>
        {
            ["AddedCount"] = result.Value.AddedCount,
            ["ModifiedCount"] = result.Value.ModifiedCount,
            ["LocationRemoved"] = result.Value.LocationRemoved,
        };
    }
}
