using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Tag.Contracts;
using Automation.Workspace.Contracts;
using Automation.Workspace.Contracts.Extensions;
using Microsoft.Extensions.Logging;

namespace Automation.Pipeline.Tools.Tags;

public class GetTagValueFromResourceTool(
    ITagApi tagApi,
    IWorkspaceApi workspaceApi,
    ILogger<GetTagValueFromResourceTool> logger
) : IResolverTool
{
    public string Key => "GetTagValueFromResource";
    public IReadOnlyList<string> Aliases => ["GetTagValueFromInspection"];
    public string Label => "Get Tag Value from Resource";
    public string? Category => "Tag & Metadata";
    public bool IsPure => true;

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new()
        {
            Id = "Target",
            Label = "Resource",
            PrimitiveType = PinPrimitiveType.EntityRef,
            Cardinality = PinCardinality.Single,
            IsRequired = true,
            Metadata = """{"type": "entity-select", "properties": {"entity": "Resource"}}"""
        },
        new()
        {
            Id = "TagId",
            Label = "Tag",
            PrimitiveType = PinPrimitiveType.EntityRef,
            Cardinality = PinCardinality.Single,
            IsRequired = true,
            Metadata = """{"type": "entity-select", "properties": {"entity": "Tag"}}"""
        }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new()
        {
            Id = "TagValues",
            Label = "Tag Values",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Array,
            IsRequired = true
        },
        new()
        {
            Id = "FirstValue",
            Label = "First Value",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single,
            IsRequired = false
        },
        new()
        {
            Id = "HasTag",
            Label = "Has Tag",
            PrimitiveType = PinPrimitiveType.Boolean,
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
                        inputs.GetValueOrDefault("EntityId") ??
                        inputs.GetValueOrDefault("target") ??
                        inputs.Values.FirstOrDefault();

        var tagObj = inputs.GetValueOrDefault("TagId") ??
                     inputs.GetValueOrDefault("tagId") ??
                     inputs.GetValueOrDefault("Tag");

        var targetGuid = EntityRefHelper.ExtractRefId(targetObj);
        if (targetGuid == null)
            throw new ArgumentException($"Invalid Target EntityRef/GUID format: '{targetObj}'");

        var tagGuid = EntityRefHelper.ExtractRefId(tagObj);
        if (tagGuid == null)
            throw new ArgumentException($"Invalid TagId EntityRef/GUID format: '{tagObj}'");

        var tGuid = targetGuid.Value;
        var tId = tagGuid.Value;

        var tagResult = await tagApi.GetTagsAsync([tId], ct);
        if (tagResult.IsFailed || !tagResult.Value.TryGetValue(tId, out var tagDto))
        {
            logger.LogWarning("Tag '{TagGuid}' not found in Tag API", tId);
        }

        var tagValues = new List<string>();

        // Lấy Resource metadata kèm TagMap từ WorkspaceApi
        var metaResult = await workspaceApi.GetMetadataDetailWithTagsAsync(tGuid, ct);
        if (metaResult.IsSuccess && metaResult.Value != null)
        {
            var rawValues = metaResult.Value.GetAllValuesByTagId(tId);
            tagValues.AddRange(rawValues.Select(v => v?.ToString() ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)));
        }

        var hasTag = tagValues.Count > 0;
        var firstValue = tagValues.FirstOrDefault() ?? string.Empty;

        return new Dictionary<string, object>
        {
            ["TagValues"] = tagValues.ToArray(),
            ["FirstValue"] = firstValue,
            ["HasTag"] = hasTag
        };
    }
}
