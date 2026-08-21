using System.Text.Json;
using Automation.Inspection.Contracts;
using Automation.Inspection.Contracts.Dtos;
using Automation.Inspection.Contracts.Extensions;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Tools;
using Automation.Workspace.Contracts;

namespace Automation.Pipeline.Engine.StructRegistry.Definitions;

public class InspectionStructDefinition(
    IInspectionApi inspectionApi,
    IWorkspaceApi workspaceApi
) : IEntityStructDefinition
{
    public string StructType => "Inspection";
    public string Label => "Inspection";

    public IReadOnlyList<PinDefinition> OutputPins =>
    [
        new()
        {
            Id = "InspectionId",
            Label = "Inspection ID",
            PrimitiveType = PinPrimitiveType.EntityRef,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "Status",
            Label = "Status",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "InspectorName",
            Label = "Inspector Name",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single
        },
        new()
        {
            Id = "MainObjects",
            Label = "Main Objects",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Array
        },
        new()
        {
            Id = "SkeletonBones",
            Label = "Skeleton Bones",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Array
        },
        new()
        {
            Id = "SummaryMessage",
            Label = "Summary Message",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single
        }
    ];

    public async Task<Dictionary<string, object>> ResolveAsync(
        object targetInput,
        ToolExecutionContext context
    )
    {
        var (type, targetGuid, isValid) = EntityRefHelper.Parse(targetInput);
        if (!isValid || targetGuid == Guid.Empty)
        {
            throw new ArgumentException($"Invalid Target Inspection Reference: '{targetInput}'");
        }

        var ct = context.CancellationToken;

        // 1. Try resolving as direct InspectionId
        InspectionDetailDto? detail = null;
        var directResult = await inspectionApi.GetInspectionWithTagsAsync(targetGuid, ct);
        if (directResult.IsSuccess)
        {
            detail = directResult.Value;
        }
        else
        {
            // 2. Fallback: Check if targetGuid is ResourceId -> resolve VersionId
            var versionId = targetGuid;
            var locResult = await workspaceApi.GetResourceLocationAsync(targetGuid, ct);
            if (locResult.IsSuccess)
            {
                versionId = locResult.Value.ResourceVersionId;
            }

            var listResult = await inspectionApi.GetInspectionsByResourceVersionAsync(versionId, ct);
            if (listResult.IsSuccess && listResult.Value.Count > 0)
            {
                detail = listResult.Value[0]; // Take latest inspection
            }
        }

        if (detail == null)
        {
            throw new InvalidOperationException($"No inspection found for target ID '{targetGuid}'.");
        }

        var insp = detail.Inspection;

        // Extract MainObjects from Data or tag
        var mainObjects = ExtractStringArray(insp.Data, "main_objects") ??
                          ExtractStringArray(insp.Data, "objects") ??
                          ExtractStringArray(insp.Data, "figures") ??
                          [];

        // Extract Bones from Data
        var bones = ExtractStringArray(insp.Data, "skeleton_bones") ??
                    ExtractStringArray(insp.Data, "bones") ??
                    [];

        return new Dictionary<string, object>
        {
            ["InspectionId"] = insp.Id,
            ["Status"] = insp.Status.ToString(),
            ["InspectorName"] = insp.InspectorName ?? string.Empty,
            ["MainObjects"] = mainObjects,
            ["SkeletonBones"] = bones,
            ["SummaryMessage"] = insp.SummaryMessage ?? string.Empty
        };
    }

    private static string[]? ExtractStringArray(JsonDocument? doc, string propertyName)
    {
        if (doc == null || doc.RootElement.ValueKind != JsonValueKind.Object)
            return null;

        if (doc.RootElement.TryGetProperty(propertyName, out var element))
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                return element.EnumerateArray()
                    .Select(e => e.ValueKind switch
                    {
                        JsonValueKind.String => e.GetString(),
                        JsonValueKind.Object when e.TryGetProperty("name", out var nameProp) => nameProp.GetString(),
                        _ => e.GetRawText()
                    })
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s!)
                    .ToArray();
            }
            if (element.ValueKind == JsonValueKind.String)
            {
                return [element.GetString()!];
            }
        }

        return null;
    }
}
