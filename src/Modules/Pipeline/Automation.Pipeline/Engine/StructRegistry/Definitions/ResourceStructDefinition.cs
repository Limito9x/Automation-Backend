using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Tools;
using Automation.Workspace.Contracts;

namespace Automation.Pipeline.Engine.StructRegistry.Definitions;

public class ResourceStructDefinition(IWorkspaceApi workspaceApi) : IEntityStructDefinition
{
    public string StructType => "Resource";
    public string Label => "Resource";

    public IReadOnlyList<PinDefinition> OutputPins =>
    [
        new()
        {
            Id = "ResourceId",
            Label = "Resource ID",
            PrimitiveType = PinPrimitiveType.EntityRef,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "ResourceVersionId",
            Label = "Resource Version ID",
            PrimitiveType = PinPrimitiveType.EntityRef,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "FileName",
            Label = "File Name",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "BaseName",
            Label = "Base Name",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "Extension",
            Label = "Extension",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "DirectoryPath",
            Label = "Directory Path",
            PrimitiveType = PinPrimitiveType.Path,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "RelativePath",
            Label = "Relative Path",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "FullPath",
            Label = "Full Path",
            PrimitiveType = PinPrimitiveType.Path,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "FileHash",
            Label = "File Hash",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single
        }
    ];

    public async Task<Dictionary<string, object>> ResolveAsync(
        object targetInput,
        ToolExecutionContext context
    )
    {
        var (type, targetId, isValid) = EntityRefHelper.Parse(targetInput);
        if (!isValid || targetId == Guid.Empty)
        {
            throw new ArgumentException($"Invalid Target Resource Reference: '{targetInput}'");
        }

        var ct = context.CancellationToken;

        // 1. Try resolving as location for agent
        var result = await workspaceApi.GetResourceLocationsAsync([targetId], context.AgentId, ct);
        ResourceLocationInfoDto? locationInfo = null;

        if (result.IsSuccess && result.Value.TryGetValue(targetId.ToString(), out var loc))
        {
            locationInfo = loc;
        }
        else
        {
            var singleResult = await workspaceApi.GetResourceLocationAsync(targetId, ct);
            if (singleResult.IsSuccess)
            {
                locationInfo = singleResult.Value;
            }
            else
            {
                var errMsg = string.Join(", ", singleResult.Errors.Select(e => e.Message));
                throw new InvalidOperationException($"Failed to resolve Resource '{targetId}': {errMsg}");
            }
        }

        var relPath = locationInfo.RelativePath ?? string.Empty;
        var fullPath = locationInfo.FullLocalPath ?? relPath;
        var fileName = Path.GetFileName(relPath);
        var baseName = Path.GetFileNameWithoutExtension(relPath);
        var extension = Path.GetExtension(relPath);
        var dirPath = Path.GetDirectoryName(fullPath)?.Replace('\\', '/') ?? string.Empty;

        return new Dictionary<string, object>
        {
            ["ResourceId"] = locationInfo.ResourceId,
            ["ResourceVersionId"] = locationInfo.ResourceVersionId,
            ["FileName"] = fileName,
            ["BaseName"] = baseName,
            ["Extension"] = extension,
            ["DirectoryPath"] = dirPath,
            ["RelativePath"] = relPath,
            ["FullPath"] = fullPath,
            ["FileHash"] = locationInfo.FileHash ?? string.Empty
        };
    }
}
