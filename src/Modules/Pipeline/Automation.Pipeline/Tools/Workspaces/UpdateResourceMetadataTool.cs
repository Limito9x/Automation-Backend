using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Workspace.Contracts;

namespace Automation.Pipeline.Tools.Workspaces;

public class UpdateResourceMetadataTool(IWorkspaceApi workspaceApi) : IResolverTool
{
    public string Key => "UpdateResourceMetadata";
    public string Label => "Update Resource Metadata";
    public string? Category => "Workspace & Files";
    public bool IsPure => false;

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new()
        {
            Id = "Target",
            Label = "Resource / Version",
            PrimitiveType = PinPrimitiveType.EntityRef,
            Cardinality = PinCardinality.Single,
            IsRequired = true,
            Metadata = """{"type": "entity-select", "properties": {"entity": "Resource"}}"""
        },
        new()
        {
            Id = "Metadata",
            Label = "Metadata JSON",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single,
            IsRequired = true
        }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new()
        {
            Id = "Success",
            Label = "Success",
            PrimitiveType = PinPrimitiveType.Boolean,
            Cardinality = PinCardinality.Single,
            IsRequired = true
        },
        new()
        {
            Id = "ResourceVersionId",
            Label = "Resource Version ID",
            PrimitiveType = PinPrimitiveType.EntityRef,
            Cardinality = PinCardinality.Single,
            IsRequired = true
        }
    ];

    public async Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    )
    {
        var ct = context.CancellationToken;

        var targetObj = inputs.GetValueOrDefault("Target") ??
                        inputs.GetValueOrDefault("ResourceVersionId") ??
                        inputs.GetValueOrDefault("Resource") ??
                        inputs.Values.FirstOrDefault();

        var metaObj = inputs.GetValueOrDefault("Metadata") ??
                      inputs.GetValueOrDefault("metadata") ??
                      inputs.GetValueOrDefault("metadata_json") ??
                      inputs.GetValueOrDefault("MetadataJson") ??
                      inputs.GetValueOrDefault("Data");

        var targetGuid = EntityRefHelper.ExtractRefId(targetObj);
        if (targetGuid == null || targetGuid == Guid.Empty)
            throw new ArgumentException($"Invalid Target Reference: '{targetObj}'");

        if (metaObj == null)
            throw new ArgumentException("Metadata JSON is required.");

        var versionId = targetGuid.Value;
        var locResult = await workspaceApi.GetResourceLocationAsync(targetGuid.Value, ct);
        if (locResult.IsSuccess)
        {
            versionId = locResult.Value.ResourceVersionId;
        }

        JsonDocument? jsonDoc = null;
        if (metaObj is JsonDocument doc)
        {
            jsonDoc = doc;
        }
        else if (metaObj is JsonElement elem)
        {
            jsonDoc = JsonDocument.Parse(elem.GetRawText());
        }
        else
        {
            var rawStr = metaObj.ToString();
            if (!string.IsNullOrWhiteSpace(rawStr))
            {
                jsonDoc = JsonDocument.Parse(rawStr);
            }
        }

        if (jsonDoc == null)
            throw new ArgumentException("Failed to parse Metadata as valid JSON.");

        var updateResult = await workspaceApi.UpdateMetadataAsync(versionId, jsonDoc, ct);
        if (updateResult.IsFailed)
        {
            throw new InvalidOperationException($"Failed to update metadata for ResourceVersion '{versionId}': {string.Join(", ", updateResult.Errors.Select(e => e.Message))}");
        }

        return new Dictionary<string, object>
        {
            ["Success"] = true,
            ["ResourceVersionId"] = EntityRefHelper.Create("ResourceVersion", versionId)
        };
    }
}
