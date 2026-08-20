using System.Text.Json;
using Automation.Inspection.Constants;
using Automation.Inspection.Infrastructure.Persistence;
using Automation.Tag.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Automation.Inspection.Infrastructure.Services;

public class InspectionApiService(InspectionDbContext db, ITagApi tagApi) : IInspectionApi
{
    public async Task<Result<IReadOnlyList<InspectionDetailDto>>> GetInspectionsByResourceVersionAsync(
        Guid resourceVersionId,
        CancellationToken ct = default
    )
    {
        var inspections = await db
            .Inspections.AsNoTracking()
            .Where(x => x.ResourceVersionId == resourceVersionId)
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

    public async Task<Result<InspectionDetailDto>> GetInspectionWithTagsAsync(
        Guid inspectionId,
        CancellationToken ct = default
    )
    {
        var inspection = await db
            .Inspections.AsNoTracking()
            .Where(x => x.Id == inspectionId)
            .Include(x => x.InspectorVersion)
                .ThenInclude(v => v.Inspector)
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
            .FirstOrDefaultAsync(ct);

        if (inspection == null)
            return Result.Fail("Inspection not found");

        var tagResult = await tagApi.GetTagsByEntityAsync(
            InspectionTag.ENTITY_TYPE,
            inspectionId,
            ct
        );

        if (tagResult.IsFailed)
            return Result.Fail(tagResult.Errors);

        var tagsByPath = tagResult.Value
            .GroupBy(t => ExtractPath(t.MetadataJson))
            .ToDictionary(g => g.Key, g => (IReadOnlyList<TagLinkDetailDto>)g.ToList());

        return Result.Ok(new InspectionDetailDto(inspection, tagsByPath));
    }

    public async Task<Result<InspectionDetailDto>> GetLatestInspectionByInspectorAsync(
        Guid resourceVersionId,
        Guid inspectorId,
        CancellationToken ct = default
    )
    {
        var inspection = await db
            .Inspections.AsNoTracking()
            .Where(x => x.ResourceVersionId == resourceVersionId && x.InspectorVersion.InspectorId == inspectorId)
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
            .FirstOrDefaultAsync(ct);

        if (inspection == null)
            return Result.Fail($"Inspection for resource version '{resourceVersionId}' and inspector '{inspectorId}' was not found.");

        var tagResult = await tagApi.GetTagsByEntityAsync(
            InspectionTag.ENTITY_TYPE,
            inspection.Id,
            ct
        );

        if (tagResult.IsFailed)
            return Result.Fail(tagResult.Errors);

        var tagsByPath = tagResult.Value
            .GroupBy(t => ExtractPath(t.MetadataJson))
            .ToDictionary(g => g.Key, g => (IReadOnlyList<TagLinkDetailDto>)g.ToList());

        return Result.Ok(new InspectionDetailDto(inspection, tagsByPath));
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
