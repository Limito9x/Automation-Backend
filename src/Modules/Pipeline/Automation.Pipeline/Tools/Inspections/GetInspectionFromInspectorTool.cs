using System.Text.Json;
using Automation.Inspection.Contracts;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Workspace.Contracts;

namespace Automation.Pipeline.Tools.Inspections;

public class GetInspectionFromInspectorTool(
    IInspectionApi inspectionApi,
    IWorkspaceApi workspaceApi
) : IResolverTool
{
    public string Key => "GetInspectionFromInspector";
    public string Label => "Get Inspection From Inspector";

    public IReadOnlyList<PinDefinition> Inputs =>
        new List<PinDefinition>
        {
            new PinDefinition
            {
                Id = "ResourceVersionId",
                Label = "Resource Version",
                PrimitiveType = PinPrimitiveType.EntityRef,
                Cardinality = PinCardinality.Single,
                IsRequired = true,
                Metadata = """{"type": "entity-select", "properties": {"entity": "Resource"}}""",
            },
            new PinDefinition
            {
                Id = "InspectorId",
                Label = "Inspector",
                PrimitiveType = PinPrimitiveType.EntityRef,
                Cardinality = PinCardinality.Single,
                IsRequired = true,
                Metadata = """{"type": "entity-select", "properties": {"entity": "Inspector"}}""",
            },
        };

    public IReadOnlyList<PinDefinition> Outputs =>
        new List<PinDefinition>
        {
            new PinDefinition
            {
                Id = "InspectionId",
                Label = "Inspection",
                PrimitiveType = PinPrimitiveType.EntityRef,
                Cardinality = PinCardinality.Single,
                IsRequired = true,
            },
            new PinDefinition
            {
                Id = "Status",
                Label = "Status",
                PrimitiveType = PinPrimitiveType.String,
                Cardinality = PinCardinality.Single,
                IsRequired = true,
            },
            new PinDefinition
            {
                Id = "SummaryMessage",
                Label = "Summary Message",
                PrimitiveType = PinPrimitiveType.String,
                Cardinality = PinCardinality.Single,
                IsRequired = false,
            },
        };

    public async Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    )
    {
        var ct = context.CancellationToken;
        var resourceVersionId = inputs.TryGetValue("ResourceVersionId", out var rVal) && rVal is Guid rGuid
            ? rGuid
            : Guid.TryParse(inputs.GetValueOrDefault("ResourceVersionId")?.ToString(), out var rParsed)
                ? rParsed
                : (Guid?)null;

        var inspectorId = inputs.TryGetValue("InspectorId", out var iVal) && iVal is Guid iGuid
            ? iGuid
            : Guid.TryParse(inputs.GetValueOrDefault("InspectorId")?.ToString(), out var iParsed)
                ? iParsed
                : (Guid?)null;

        if (resourceVersionId == null || inspectorId == null)
            throw new ArgumentException("ResourceVersionId and InspectorId are required.");

        var result = await inspectionApi.GetLatestInspectionByInspectorAsync(
            resourceVersionId.Value,
            inspectorId.Value,
            ct
        );

        // Fallback: If not found directly, resolve real versionId via WorkspaceApi (in case ResourceId was passed)
        if (result.IsFailed)
        {
            var locResult = await workspaceApi.GetResourceLocationAsync(resourceVersionId.Value, ct);
            if (locResult.IsSuccess && locResult.Value.ResourceVersionId != resourceVersionId.Value)
            {
                result = await inspectionApi.GetLatestInspectionByInspectorAsync(
                    locResult.Value.ResourceVersionId,
                    inspectorId.Value,
                    ct
                );
            }
        }

        if (result.IsFailed)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Message)));

        var inspection = result.Value.Inspection;

        return new Dictionary<string, object>
        {
            ["InspectionId"] = inspection.Id,
            ["Status"] = inspection.Status.ToString(),
            ["SummaryMessage"] = inspection.SummaryMessage ?? string.Empty,
        };
    }
}
