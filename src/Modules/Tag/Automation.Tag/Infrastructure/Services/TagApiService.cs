using Automation.Tag.Contracts;
using Automation.Tag.Contracts.Dtos;
using Automation.Tag.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.Tag.Infrastructure.Services;

public class TagApiService(TagDbContext db) : ITagApi
{
    public async Task<Result<IReadOnlyList<TagLinkDetailDto>>> GetTagsByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken ct = default
    )
    {
        var links = await db
            .TagLinks.AsNoTracking()
            .Include(x => x.Tag)
                .ThenInclude(x => x.TagGroup)
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderBy(x => x.Tag.TagGroup.Scope)
            .ThenBy(x => x.Tag.TagGroup.Name)
            .ThenBy(x => x.Tag.Name)
            .ToListAsync(ct);

        var result = links
            .Select(l => new TagLinkDetailDto(
                l.Id,
                l.TagId,
                l.Tag.Name,
                l.Tag.Color,
                l.Tag.TagGroupId,
                l.Tag.TagGroup.Scope,
                l.Tag.TagGroup.Name,
                l.Metadata?.RootElement.ToString()
            ))
            .ToList();

        return Result.Ok<IReadOnlyList<TagLinkDetailDto>>(result);
    }

    public async Task<
        Result<IReadOnlyDictionary<Guid, IReadOnlyList<TagLinkDetailDto>>>
    > GetTagsByEntitiesAsync(
        string entityType,
        IEnumerable<Guid> entityIds,
        CancellationToken ct = default
    )
    {
        var ids = entityIds.ToHashSet();
        if (ids.Count == 0)
            return Result.Ok<IReadOnlyDictionary<Guid, IReadOnlyList<TagLinkDetailDto>>>(
                new Dictionary<Guid, IReadOnlyList<TagLinkDetailDto>>()
            );

        var links = await db
            .TagLinks.AsNoTracking()
            .Include(x => x.Tag)
                .ThenInclude(x => x.TagGroup)
            .Where(x => x.EntityType == entityType && ids.Contains(x.EntityId))
            .OrderBy(x => x.Tag.TagGroup.Scope)
            .ThenBy(x => x.Tag.TagGroup.Name)
            .ThenBy(x => x.Tag.Name)
            .ToListAsync(ct);

        var grouped = links
            .GroupBy(l => l.EntityId)
            .ToDictionary(
                g => g.Key,
                g =>
                    (IReadOnlyList<TagLinkDetailDto>)
                        g.Select(l => new TagLinkDetailDto(
                                l.Id,
                                l.TagId,
                                l.Tag.Name,
                                l.Tag.Color,
                                l.Tag.TagGroupId,
                                l.Tag.TagGroup.Scope,
                                l.Tag.TagGroup.Name,
                                l.Metadata?.RootElement.ToString()
                            ))
                            .ToList()
            );

        return Result.Ok<IReadOnlyDictionary<Guid, IReadOnlyList<TagLinkDetailDto>>>(grouped);
    }

    public async Task<Result<IReadOnlyList<TagGroupDto>>> GetTagGroupsByScopeAsync(
        string scope,
        CancellationToken ct = default
    )
    {
        var groups = await db
            .TagGroups.AsNoTracking()
            .Where(x => x.Scope == scope)
            .OrderBy(x => x.Name)
            .Select(x => new TagGroupDto(x.Id, x.Scope, x.Name, x.CreatedAt))
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<TagGroupDto>>(groups);
    }

    public async Task<Result<IReadOnlyList<TagDto>>> GetTagsByGroupAsync(
        Guid tagGroupId,
        CancellationToken ct = default
    )
    {
        var tags = await db
            .TagItems.AsNoTracking()
            .Where(x => x.TagGroupId == tagGroupId)
            .OrderBy(x => x.Name)
            .Select(x => new TagDto(x.Id, x.TagGroupId, x.Name, x.Color, x.CreatedAt))
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<TagDto>>(tags);
    }

    public async Task<Result<IReadOnlyDictionary<Guid, TagDto>>> GetTagsAsync(
        IReadOnlyList<Guid> tagIds,
        CancellationToken ct = default
    )
    {
        var tags = await db
            .TagItems.AsNoTracking()
            .Where(x => tagIds.Contains(x.Id))
            .ToDictionaryAsync(
                x => x.Id,
                x => new TagDto(x.Id, x.TagGroupId, x.Name, x.Color, x.CreatedAt),
                ct
            );

        return Result.Ok<IReadOnlyDictionary<Guid, TagDto>>(tags);
    }

    public async Task<Result> UpdateTagLinksMetadataAsync(
        IReadOnlyDictionary<Guid, string> tagLinkIdToMetadataJson,
        CancellationToken ct = default
    )
    {
        if (tagLinkIdToMetadataJson.Count == 0)
            return Result.Ok();

        var linkIds = tagLinkIdToMetadataJson.Keys.ToList();
        var links = await db.TagLinks
            .Where(x => linkIds.Contains(x.Id))
            .ToListAsync(ct);

        foreach (var link in links)
        {
            if (tagLinkIdToMetadataJson.TryGetValue(link.Id, out var jsonStr))
            {
                var doc = string.IsNullOrWhiteSpace(jsonStr) ? null : System.Text.Json.JsonDocument.Parse(jsonStr);
                link.UpdateMetadata(doc);
            }
        }

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
