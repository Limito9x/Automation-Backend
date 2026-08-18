using System.Text.Json;
using Automation.Inspection.Constants;
using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Automation.Tag.Contracts;
using Automation.Tag.Contracts.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspections.GetInspectionsByResourceVersion;

[NonTransactional]
public class GetInspectionsByResourceVersionHandler(InspectionDbContext db, ITagApi tagApi)
{
    public async Task<Result<IReadOnlyList<InspectionDetailDto>>> HandleAsync(
        GetInspectionsByResourceVersionQuery query,
        CancellationToken ct
    )
    {
        var inspections = await db
            .Inspections.AsNoTracking()
            .Where(x => x.ResourceVersionId == query.ResourceVersionId)
            .Include(x => x.InspectorVersion)
                .ThenInclude(v => v.Inspector)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new InspectionDto(
                x.Id,
                x.ResourceVersionId,
                x.InspectorVersionId,
                x.InspectorVersion.Inspector.Name,
                x.InspectorVersion.Inspector.Key,
                x.InspectorVersion.Version,
                x.InspectorVersion.Inspector.ExecutorKey,
                x.Status,
                x.Data,
                x.ExecutionTimeMs,
                x.SummaryMessage,
                x.InspectedAt,
                x.CreatedAt
            ))
            .ToListAsync(ct);

        var inspectionIds = inspections.Select(x => x.Id).ToHashSet();

        var tagResult = await tagApi.GetTagsByEntitiesAsync(
            InspectionTag.ENTITY_TYPE,
            inspectionIds,
            ct
        );

        if (tagResult.IsFailed)
            return Result.Fail(tagResult.Errors);

        var result = new List<InspectionDetailDto>(inspections.Count);
        foreach (var inspection in inspections)
        {
            var tagsByPath = tagResult.Value.TryGetValue(inspection.Id, out var tags)
                ? tags.GroupBy(t => ExtractPath(t.MetadataJson))
                    .ToDictionary(g => g.Key, g => (IReadOnlyList<TagLinkDetailDto>)g.ToList())
                : new Dictionary<string, IReadOnlyList<TagLinkDetailDto>>();

            result.Add(new InspectionDetailDto(inspection, tagsByPath));
        }

        return Result.Ok<IReadOnlyList<InspectionDetailDto>>(result);
    }

    private static string ExtractPath(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
            return string.Empty;

        try
        {
            using var doc = JsonDocument.Parse(metadataJson);
            if (
                doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty("path", out var pathProp)
            )
            {
                return pathProp.GetString() ?? string.Empty;
            }
        }
        catch (JsonException)
        {
            return string.Empty;
        }
        return string.Empty;
    }
}
