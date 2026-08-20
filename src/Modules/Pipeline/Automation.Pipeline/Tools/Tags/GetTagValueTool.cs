using Automation.Inspection.Contracts;
using Automation.Inspection.Contracts.Extensions;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Tag.Contracts;
using Automation.Workspace.Contracts;
using Microsoft.Extensions.Logging;

namespace Automation.Pipeline.Tools.Tags;

public class GetTagValueTool(
    IInspectionApi inspectionApi,
    ITagApi tagApi,
    IWorkspaceApi workspaceApi,
    Microsoft.Extensions.Logging.ILogger<GetTagValueTool> logger
) : IResolverTool
{
    public string Key => "GetTagValue";
    public string Label => "Get Tag Value";

    public IReadOnlyList<PinDefinition> Inputs =>
        new List<PinDefinition>
        {
            new PinDefinition
            {
                Id = "EntityType",
                Label = "Entity Type",
                PrimitiveType = PinPrimitiveType.String,
                Cardinality = PinCardinality.Single,
                IsRequired = true,
                DefaultValue = "Inspection",
            },
            new PinDefinition
            {
                Id = "EntityId",
                Label = "Entity",
                PrimitiveType = PinPrimitiveType.EntityRef,
                Cardinality = PinCardinality.Single,
                IsRequired = true,
                Metadata = """{"type": "entity-select", "properties": {"entity": "Inspection"}}""",
            },
            new PinDefinition
            {
                Id = "TagId",
                Label = "Tag",
                PrimitiveType = PinPrimitiveType.EntityRef,
                Cardinality = PinCardinality.Single,
                IsRequired = true,
                Metadata = """{"type": "entity-select", "properties": {"entity": "Tag"}}""",
            },
        };

    public IReadOnlyList<PinDefinition> Outputs =>
        new List<PinDefinition>
        {
            new PinDefinition
            {
                Id = "TagValue",
                Label = "Tag Values",
                PrimitiveType = PinPrimitiveType.String,
                Cardinality = PinCardinality.Array,
                IsRequired = true,
            },
        };

    public async Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    )
    {
        var ct = context.CancellationToken;

        var entityType = inputs.GetValueOrDefault("EntityType")?.ToString() ?? "Inspection";

        var entityId = inputs.TryGetValue("EntityId", out var eVal) && eVal is Guid eGuid
            ? eGuid
            : Guid.TryParse(inputs.GetValueOrDefault("EntityId")?.ToString(), out var eParsed)
                ? eParsed
                : (Guid?)null;

        var tagId = inputs.TryGetValue("TagId", out var tVal) && tVal is Guid tGuid
            ? tGuid
            : Guid.TryParse(inputs.GetValueOrDefault("TagId")?.ToString(), out var tParsed)
                ? tParsed
                : (Guid?)null;

        if (entityId == null || tagId == null)
        {
            var received = string.Join(", ", inputs.Select(kv => $"'{kv.Key}': '{kv.Value}'"));
            throw new ArgumentException($"EntityId and TagId are required. Received inputs: [{received}]");
        }

        var tag = await tagApi.GetTagsAsync([tagId.Value], ct);
        if (tag.IsFailed || !tag.Value.TryGetValue(tagId.Value, out var tagDto))
            throw new Exception($"Tag {tagId.Value} not found");

        logger.LogInformation("🔍 [GetTagValueTool] Querying for Tag '{TagName}' ({TagId}) on EntityId '{EntityId}'",
            tagDto.Name, tagId.Value, entityId.Value);

        var tagValues = new List<object>();
        string debugTagMapSummary = string.Empty;
        string debugInspectionData = string.Empty;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n=======================================================");
        Console.WriteLine($"🔍 [GetTagValueTool] Searching for Tag '{tagDto.Name}' ({tagId.Value}) on Entity '{entityId.Value}'");

        // 1. Try resolving as direct InspectionId
        var result = await inspectionApi.GetInspectionWithTagsAsync(entityId.Value, ct);
        if (result.IsSuccess)
        {
            var detail = result.Value;
            debugTagMapSummary = string.Join("; ", detail.TagMap.Select(kv => $"{kv.Key} -> [{string.Join(",", kv.Value.Select(t => $"{t.TagName}:{t.TagId}"))}]"));
            debugInspectionData = detail.Inspection.Data?.RootElement.ToString() ?? "{}";

            Console.WriteLine($"📋 [GetTagValueTool] Found Inspection '{detail.Inspection.Id}' ({detail.Inspection.InspectorName})");
            Console.WriteLine($"   TagMap: {debugTagMapSummary}");
            Console.WriteLine($"   Data Payload: {debugInspectionData}");

            tagValues.AddRange(detail.GetAllValuesByTagId(tagId.Value));
        }
        else
        {
            Console.WriteLine($"⚠️ [GetTagValueTool] '{entityId.Value}' is not an InspectionId. Querying Resource/ResourceVersion...");

            // 2. Fallback: Check if EntityId is a ResourceVersionId or ResourceId
            var versionId = entityId.Value;
            var locResult = await workspaceApi.GetResourceLocationAsync(entityId.Value, ct);
            if (locResult.IsSuccess)
            {
                versionId = locResult.Value.ResourceVersionId;
                Console.WriteLine($"📌 [GetTagValueTool] Resolved Resource '{entityId.Value}' -> Version '{versionId}'");
            }

            var inspListResult = await inspectionApi.GetInspectionsByResourceVersionAsync(versionId, ct);
            if (inspListResult.IsSuccess && inspListResult.Value.Count > 0)
            {
                Console.WriteLine($"📋 [GetTagValueTool] Found {inspListResult.Value.Count} Inspection(s) for Version '{versionId}':");

                // Take tags from latest inspection(s)
                foreach (var insp in inspListResult.Value)
                {
                    var mapSummary = string.Join("; ", insp.TagMap.Select(kv => $"{kv.Key} -> [{string.Join(",", kv.Value.Select(t => $"{t.TagName}:{t.TagId}"))}]"));
                    Console.WriteLine($"   - Inspection [{insp.Inspection.Id} / {insp.Inspection.InspectorName}]");
                    Console.WriteLine($"     TagMap: {mapSummary}");
                    Console.WriteLine($"     Data: {insp.Inspection.Data?.RootElement.ToString() ?? "{}"}");

                    var vals = insp.GetAllValuesByTagId(tagId.Value);
                    if (vals.Count > 0)
                    {
                        tagValues.AddRange(vals);
                        debugTagMapSummary = mapSummary;
                        debugInspectionData = insp.Inspection.Data?.RootElement.ToString() ?? "{}";
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine($"❌ [GetTagValueTool] No inspections found for Resource/Version '{versionId}'!");
            }
        }

        Console.WriteLine($"🎯 [GetTagValueTool] Matched TagValues result: [{string.Join(", ", tagValues)}] (Count={tagValues.Count})");
        Console.WriteLine($"=======================================================\n");
        Console.ResetColor();

        return new Dictionary<string, object>
        {
            ["TagValue"] = tagValues.Select(x => x.ToString() ?? string.Empty).ToArray(),
            ["DebugTagMap"] = debugTagMapSummary,
            ["DebugPayload"] = debugInspectionData
        };
    }
}
