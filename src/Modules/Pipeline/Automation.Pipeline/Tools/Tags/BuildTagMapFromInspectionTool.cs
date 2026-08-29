using System.Text.Json;
using Automation.Inspection.Contracts;
using Automation.Inspection.Contracts.Dtos;
using Automation.Inspection.Contracts.Extensions;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Tag.Contracts;
using Automation.Workspace.Contracts;
using Microsoft.Extensions.Logging;

namespace Automation.Pipeline.Tools.Tags;

public class BuildTagMapFromInspectionTool(
    IInspectionApi inspectionApi,
    IWorkspaceApi workspaceApi,
    ILogger<BuildTagMapFromInspectionTool> logger
) : IResolverTool
{
    public string Key => "BuildTagMapFromInspection";
    public string Label => "Build Tag Map from Inspection";
    public string? Category => "Inspection & Tag";
    public bool IsPure => true;

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new()
        {
            Id = "Target",
            Label = "Inspection / Resource",
            PrimitiveType = PinPrimitiveType.EntityRef,
            Cardinality = PinCardinality.Single,
            IsRequired = true,
            Metadata = """{"type": "entity-select", "properties": {"entity": "Inspection"}}"""
        }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new()
        {
            Id = "TagMap",
            Label = "Tag Map",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Map,
            IsRequired = true
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

        // 1. Resolve inspection data once
        InspectionDetailDto? activeInspection = null;
        var inspResult = await inspectionApi.GetInspectionWithTagsAsync(tGuid, ct);
        if (inspResult.IsSuccess)
        {
            activeInspection = inspResult.Value;
        }
        else
        {
            var versionId = tGuid;
            var locResult = await workspaceApi.GetResourceLocationAsync(tGuid, ct);
            if (locResult.IsSuccess)
            {
                versionId = locResult.Value.ResourceVersionId;
            }

            var listResult = await inspectionApi.GetInspectionsByResourceVersionAsync(versionId, ct);
            if (listResult.IsSuccess && listResult.Value.Count > 0)
            {
                activeInspection = listResult.Value.FirstOrDefault();
            }
        }

        if (activeInspection == null)
        {
            logger.LogWarning("No active inspection found for target GUID: {TargetGuid}", tGuid);
        }

        var tagMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var missingSlots = new List<string>();

        // 2. Iterate through all other dynamic input pins (each pin represents a slot -> tag)
        var reservedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Target", "target", "EntityId", "entityId", "Entity", "entity", "exec_in",
            "Inspection / Resource", "Inspection/Resource", "Inspection", "Resource", "Inspector"
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
                }
                else
                {
                    missingSlots.Add(pinKey);
                }
                continue;
            }

            var tId = tagGuid.Value;
            var tagValue = string.Empty;

            if (activeInspection != null)
            {
                var rawValues = activeInspection.GetAllValuesByTagId(tId);
                tagValue = rawValues
                    .Select(v => v?.ToString() ?? string.Empty)
                    .FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(tagValue))
            {
                tagMap[pinKey] = tagValue;
            }
            else
            {
                missingSlots.Add(pinKey);
            }
        }

        // 3. AUTOMATIC FALLBACK: If user didn't configure custom dynamic pins, automatically extract all tags from activeInspection!
        if (!hasCustomDynamicPins && activeInspection != null)
        {
            // Auto-extract from activeInspection.TagMap
            if (activeInspection.TagMap != null && activeInspection.TagMap.Count > 0)
            {
                foreach (var (path, tagLinks) in activeInspection.TagMap)
                {
                    if (tagLinks == null || tagLinks.Count == 0) continue;

                    var val = activeInspection.Inspection.Data != null
                        ? activeInspection.Inspection.Data.RootElement.ExtractJsonValue(path)
                        : null;

                    var valStr = val switch
                    {
                        IEnumerable<object> list => list.Select(x => x?.ToString()).FirstOrDefault(x => !string.IsNullOrEmpty(x)),
                        JsonElement je when je.ValueKind == JsonValueKind.String => je.GetString(),
                        _ => val?.ToString()
                    };

                    if (string.IsNullOrEmpty(valStr)) continue;

                    foreach (var tag in tagLinks)
                    {
                        if (!string.IsNullOrWhiteSpace(tag.TagName))
                        {
                            tagMap[tag.TagName] = valStr;
                        }
                    }
                }
            }

            // Secondary Fallback: Extract from main_objects or objects array in Inspection.Data
            if (tagMap.Count == 0 && activeInspection.Inspection?.Data != null)
            {
                var root = activeInspection.Inspection.Data.RootElement;
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
                        }
                    }
                }
            }
        }

        logger.LogInformation("BuildTagMapFromInspection generated TagMap with {Count} entries: {Json}",
            tagMap.Count, System.Text.Json.JsonSerializer.Serialize(tagMap));

        return new Dictionary<string, object>
        {
            ["TagMap"] = tagMap,
            ["MissingSlots"] = missingSlots.ToArray()
        };
    }
}
