using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Workspace.Contracts;

namespace Automation.Pipeline.Tools.Workspaces;

public class GetResourcePathTool(IWorkspaceApi workspaceApi) : IResolverTool
{
    public string Key => "get_resource_path";
    public string Label => "Get Resource Path";
    public string Category => "Workspaces";

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new()
        {
            Id = "ResourceVersionId",
            Label = "Resource Version",
            PrimitiveType = PinPrimitiveType.EntityRef,
            Cardinality = PinCardinality.Single,
            IsRequired = true
        }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new()
        {
            Id = "FullPath",
            Label = "Full Path",
            PrimitiveType = PinPrimitiveType.Path,
            Cardinality = PinCardinality.Single,
            IsRequired = true
        },
        new()
        {
            Id = "RelativePath",
            Label = "Relative Path",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single,
            IsRequired = false
        },
        new()
        {
            Id = "FileHash",
            Label = "File Hash",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single,
            IsRequired = false
        }
    ];

    public async Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    )
    {
        var rvObj = inputs.GetValueOrDefault("ResourceVersionId") ??
                    inputs.GetValueOrDefault("ResourceVersion") ??
                    inputs.GetValueOrDefault("resource_version_id") ??
                    inputs.GetValueOrDefault("resource_version") ??
                    inputs.GetValueOrDefault("Resource Version") ??
                    inputs.GetValueOrDefault("resource") ??
                    inputs.GetValueOrDefault("Resource") ??
                    inputs.Values.FirstOrDefault(v => v != null && Guid.TryParse(v.ToString(), out _)) ??
                    (inputs.Count == 1 ? inputs.Values.FirstOrDefault() : null);

        if (rvObj == null)
        {
            var received = string.Join(", ", inputs.Select(kv => $"'{kv.Key}': '{kv.Value}'"));
            throw new ArgumentException($"ResourceVersionId is required. Received inputs: [{received}]");
        }

        var rvStr = rvObj.ToString();
        if (!Guid.TryParse(rvStr, out var rvId))
        {
            throw new ArgumentException($"Invalid ResourceVersionId GUID format: '{rvStr}'");
        }

        var result = await workspaceApi.GetResourceLocationsAsync([rvId], context.AgentId, context.CancellationToken);
        if (result.IsFailed || !result.Value.TryGetValue(rvId.ToString(), out var locationInfo) || locationInfo == null)
        {
            // Fallback to origin location
            var singleResult = await workspaceApi.GetResourceLocationAsync(rvId, context.CancellationToken);
            if (singleResult.IsFailed)
            {
                throw new InvalidOperationException($"Failed to resolve location for ResourceVersion '{rvId}': {string.Join(", ", singleResult.Errors.Select(e => e.Message))}");
            }
            locationInfo = singleResult.Value;
        }

        var fullPath = locationInfo.FullLocalPath ?? locationInfo.RelativePath;

        return new Dictionary<string, object>
        {
            ["FullPath"] = fullPath,
            ["RelativePath"] = locationInfo.RelativePath,
            ["FileHash"] = locationInfo.FileHash ?? string.Empty
        };
    }
}
