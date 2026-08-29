using System.Text.Json;
using Automation.Inspection.Contracts;
using Automation.Inspection.Contracts.Dtos;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Workspace.Contracts;

namespace Automation.Pipeline.Tools.Inspections;

/// <summary>
/// Tool lấy thông tin Inspection của Resource theo Inspector tương ứng (Pure Data Tool).
/// </summary>
public class GetResourceInspectionTool(IInspectionApi inspectionApi, IWorkspaceApi workspaceApi)
    : IResolverTool
{
    public string Key => "GetResourceInspection";
    public string Label => "Get Inspection";
    public string? Category => "Inspection & Tag";
    public bool IsPure => true;

    public IReadOnlyList<PinDefinition> Inputs =>
        [
            new()
            {
                Id = "Resource",
                Label = "Resource",
                PrimitiveType = PinPrimitiveType.EntityRef,
                Cardinality = PinCardinality.Single,
                IsRequired = true,
                Metadata = """{"type": "entity-select", "properties": {"entity": "Resource"}}""",
            },
            new()
            {
                Id = "Inspector",
                Label = "Inspector",
                PrimitiveType = PinPrimitiveType.EntityRef,
                Cardinality = PinCardinality.Single,
                IsRequired = false,
                Metadata = """{"type": "entity-select", "properties": {"entity": "Inspector"}}""",
            },
        ];

    public IReadOnlyList<PinDefinition> Outputs =>
        [
            new()
            {
                Id = "Inspection",
                Label = "Inspection Entity",
                PrimitiveType = PinPrimitiveType.EntityRef,
                Cardinality = PinCardinality.Single,
            },
            new()
            {
                Id = "InspectionId",
                Label = "Inspection ID",
                PrimitiveType = PinPrimitiveType.EntityRef,
                Cardinality = PinCardinality.Single,
            },
            new()
            {
                Id = "InspectorName",
                Label = "Inspector Name",
                PrimitiveType = PinPrimitiveType.String,
                Cardinality = PinCardinality.Single,
            },
        ];

    public async Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    )
    {
        var resourceInput =
            inputs.GetValueOrDefault("Resource")
            ?? inputs.GetValueOrDefault("resource")
            ?? inputs.GetValueOrDefault("Target");

        if (resourceInput == null)
        {
            throw new ArgumentException("Resource input is required.");
        }

        var (resType, resId, isResValid) = EntityRefHelper.Parse(resourceInput);
        if (!isResValid || resId == Guid.Empty)
        {
            throw new ArgumentException($"Invalid Resource Reference: '{resourceInput}'");
        }

        var ct = context.CancellationToken;

        // Resolve ResourceVersionId if passed ResourceId
        var versionId = resId;
        var locResult = await workspaceApi.GetResourceLocationAsync(resId, ct);
        if (locResult.IsSuccess)
        {
            versionId = locResult.Value.ResourceVersionId;
        }

        InspectionDetailDto? detail = null;

        var inspectorInput =
            inputs.GetValueOrDefault("Inspector") ?? inputs.GetValueOrDefault("inspector");

        if (inspectorInput != null)
        {
            var (inspType, inspectorId, isInspValid) = EntityRefHelper.Parse(inspectorInput);
            if (isInspValid && inspectorId != Guid.Empty)
            {
                var inspectorResult = await inspectionApi.GetLatestInspectionByInspectorAsync(
                    versionId,
                    inspectorId,
                    ct
                );
                if (inspectorResult.IsSuccess)
                {
                    detail = inspectorResult.Value;
                }
            }
        }

        // Fallback: If not found by specific inspector or no inspector specified -> take latest inspection
        if (detail == null)
        {
            var listResult = await inspectionApi.GetInspectionsByResourceVersionAsync(
                versionId,
                ct
            );
            if (listResult.IsSuccess && listResult.Value.Count > 0)
            {
                detail = listResult.Value[0];
            }
        }

        if (detail == null)
        {
            throw new InvalidOperationException($"No inspection found for Resource '{resId}'.");
        }

        var insp = detail.Inspection;

        var mainObjects =
            ExtractStringArray(insp.Data, "main_objects")
            ?? ExtractStringArray(insp.Data, "objects")
            ?? ExtractStringArray(insp.Data, "figures")
            ?? [];

        var bones =
            ExtractStringArray(insp.Data, "skeleton_bones")
            ?? ExtractStringArray(insp.Data, "bones")
            ?? [];

        return new Dictionary<string, object>
        {
            ["Inspection"] = EntityRefHelper.Create("Inspection", insp.Id),
            ["InspectionId"] = insp.Id,
            ["InspectorName"] = insp.InspectorName ?? string.Empty,
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
                return element
                    .EnumerateArray()
                    .Select(e =>
                        e.ValueKind switch
                        {
                            JsonValueKind.String => e.GetString(),
                            JsonValueKind.Object when e.TryGetProperty("name", out var nameProp) =>
                                nameProp.GetString(),
                            _ => e.GetRawText(),
                        }
                    )
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
