using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Tag.Contracts;
using Automation.Workspace.Contracts;
using Automation.Workspace.Contracts.Dtos;
using Automation.Workspace.Contracts.Extensions;
using Microsoft.Extensions.Logging;

namespace Automation.Pipeline.Tools.Tags;

public class BuildTagMapFromResourceTool(
    IWorkspaceApi workspaceApi,
    ILogger<BuildTagMapFromResourceTool> logger
) : IResolverTool
{
    public string Key => "BuildTagMapFromResource";
    public IReadOnlyList<string> Aliases => ["BuildTagMapFromInspection"];
    public string Label => "Build Tag Map from Resource";
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
            Id = "TagGroupId",
            Label = "Tag Group",
            PrimitiveType = PinPrimitiveType.EntityRef,
            Cardinality = PinCardinality.Single,
            IsRequired = false,
            Metadata = """{"type": "entity-select", "properties": {"entity": "TagGroup"}}"""
        }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new()
        {
            Id = "ValueMap",
            Label = "Value Map",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Map,
            IsRequired = true
        },
        new()
        {
            Id = "PathMap",
            Label = "Path Map",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Map,
            IsRequired = false
        },
        new()
        {
            Id = "MissingSlots",
            Label = "Missing Slots",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Array,
            IsRequired = false
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

        var targetGuid = EntityRefHelper.ExtractRefId(targetObj);
        if (targetGuid == null)
            throw new ArgumentException($"Invalid Target EntityRef/GUID format: '{targetObj}'");

        var tGuid = targetGuid.Value;

        // 1. Resolve resource metadata with tags
        ResourceMetadataDetailDto? activeResourceMeta = null;
        var metaResult = await workspaceApi.GetMetadataDetailWithTagsAsync(tGuid, ct);
        if (metaResult.IsSuccess && metaResult.Value != null)
        {
            activeResourceMeta = metaResult.Value;
        }

        if (activeResourceMeta == null)
        {
            logger.LogWarning("No active metadata found for target Resource GUID: {TargetGuid}", tGuid);
        }

        var tagGroupObj = inputs.GetValueOrDefault("TagGroupId") ??
                          inputs.GetValueOrDefault("tagGroupId") ??
                          inputs.GetValueOrDefault("TagGroup") ??
                          inputs.GetValueOrDefault("tagGroup");
        var tagGroupGuid = EntityRefHelper.ExtractRefId(tagGroupObj);

        var tagMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var valueMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var pathMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var pathToTagMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var missingSlots = new List<string>();

        // 2. Iterate through all other dynamic input pins (each pin represents a slot -> tag)
        var reservedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Target", "target", "EntityId", "entityId", "Entity", "entity", "exec_in",
            "Inspection / Resource", "Inspection/Resource", "Inspection", "Resource", "Inspector",
            "TagGroupId", "tagGroupId", "TagGroup", "tagGroup"
        };

        var hasCustomDynamicPins = false;

        foreach (var (pinKey, pinVal) in inputs)
        {
            if (reservedKeys.Contains(pinKey) || pinKey.Contains("resource", StringComparison.OrdinalIgnoreCase) && pinKey.Contains("inspection", StringComparison.OrdinalIgnoreCase) || pinVal == null)
                continue;

            hasCustomDynamicPins = true;
            var tagGuid = EntityRefHelper.ExtractRefId(pinVal);
            if (tagGuid == null)
            {
                // If pinVal is already a raw string value (e.g. manual literal), assign directly
                var rawStr = pinVal.ToString()?.Trim();
                if (!string.IsNullOrEmpty(rawStr))
                {
                    tagMap[pinKey] = rawStr;
                    valueMap[pinKey] = rawStr;
                }
                else
                {
                    missingSlots.Add(pinKey);
                }
                continue;
            }

            var tId = tagGuid.Value;
            var tagValue = string.Empty;

            if (activeResourceMeta != null)
            {
                var rawValues = activeResourceMeta.GetAllValuesByTagId(tId);
                tagValue = rawValues
                    .Select(v => v?.ToString() ?? string.Empty)
                    .FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(tagValue))
            {
                tagMap[pinKey] = tagValue;
                valueMap[pinKey] = tagValue;
            }
            else
            {
                missingSlots.Add(pinKey);
            }
        }

        // 3. AUTOMATIC FALLBACK: If user didn't configure custom dynamic pins, automatically extract all tags from activeResourceMeta!
        if (!hasCustomDynamicPins && activeResourceMeta != null)
        {
            // Auto-extract from activeResourceMeta.TagMap
            if (activeResourceMeta.TagMap != null && activeResourceMeta.TagMap.Count > 0)
            {
                foreach (var (path, tagLinks) in activeResourceMeta.TagMap)
                {
                    if (tagLinks == null || tagLinks.Count == 0) continue;

                    var val = activeResourceMeta.Metadata != null
                        ? activeResourceMeta.Metadata.RootElement.ExtractJsonValue(path)
                        : null;

                    var valStr = val switch
                    {
                        IEnumerable<object> list => list.Select(x => x?.ToString()).FirstOrDefault(x => !string.IsNullOrEmpty(x)),
                        JsonElement je when je.ValueKind == JsonValueKind.String => je.GetString(),
                        _ => val?.ToString()
                    };

                    foreach (var tag in tagLinks)
                    {
                        if (tagGroupGuid != null && tag.TagGroupId != tagGroupGuid.Value)
                            continue;

                        if (!string.IsNullOrWhiteSpace(tag.TagName))
                        {
                            var finalVal = !string.IsNullOrWhiteSpace(valStr) ? valStr : tag.TagName;
                            tagMap[tag.TagName] = finalVal;
                            valueMap[tag.TagName] = finalVal;
                            pathMap[tag.TagName] = path;
                            pathMap[path] = tag.TagName;
                            pathToTagMap[path] = tag.TagName;
                        }
                    }
                }
            }

            // Secondary Fallback: Extract from main_objects or objects array in Metadata
            if (tagMap.Count == 0 && activeResourceMeta.Metadata != null)
            {
                var root = activeResourceMeta.Metadata.RootElement;
                JsonElement objectsElem = default;

                if (root.TryGetProperty("main_objects", out var mo) && mo.ValueKind == JsonValueKind.Array)
                {
                    objectsElem = mo;
                }
                else if (root.TryGetProperty("objects", out var ob) && ob.ValueKind == JsonValueKind.Array)
                {
                    objectsElem = ob;
                }
                else if (root.TryGetProperty("figures", out var fig) && fig.ValueKind == JsonValueKind.Array)
                {
                    objectsElem = fig;
                }

                if (objectsElem.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in objectsElem.EnumerateArray())
                    {
                        var name = item.ValueKind switch
                        {
                            JsonValueKind.String => item.GetString(),
                            JsonValueKind.Object when item.TryGetProperty("name", out var np) => np.GetString(),
                            _ => null
                        };

                        if (!string.IsNullOrEmpty(name))
                        {
                            tagMap[name] = name;
                            valueMap[name] = name;
                        }
                    }
                }
            }
        }

        logger.LogInformation("BuildTagMapFromResource generated TagMap with {Count} entries: {Json}",
            tagMap.Count, System.Text.Json.JsonSerializer.Serialize(tagMap));

        return new Dictionary<string, object>
        {
            ["TagMap"] = tagMap,
            ["ValueMap"] = valueMap,
            ["PathMap"] = pathMap,
            ["PathToTagMap"] = pathToTagMap,
            ["MissingSlots"] = missingSlots.ToArray()
        };
    }
}
