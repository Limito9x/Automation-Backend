using Automation.Tag.Contracts.Dtos;
using FluentResults;

namespace Automation.Tag.Contracts;

/// <summary>
/// Public API for other modules to query tags.
/// All methods are read-only — mutation goes through Tag endpoints directly.
/// </summary>
public interface ITagApi
{
    /// <summary>Get all tag links for a single entity, including tag + group info.</summary>
    Task<Result<IReadOnlyList<TagLinkDetailDto>>> GetTagsByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken ct = default
    );

    /// <summary>Get tags for multiple entities of the same type (bulk).</summary>
    Task<Result<IReadOnlyDictionary<Guid, IReadOnlyList<TagLinkDetailDto>>>> GetTagsByEntitiesAsync(
        string entityType,
        IEnumerable<Guid> entityIds,
        CancellationToken ct = default
    );

    /// <summary>Get all tag groups for a given scope.</summary>
    Task<Result<IReadOnlyList<TagGroupDto>>> GetTagGroupsByScopeAsync(
        string scope,
        CancellationToken ct = default
    );

    /// <summary>Get all tags belonging to a tag group.</summary>
    Task<Result<IReadOnlyList<TagDto>>> GetTagsByGroupAsync(
        Guid tagGroupId,
        CancellationToken ct = default
    );

    Task<Result<IReadOnlyDictionary<Guid, TagDto>>> GetTagsAsync(
        IReadOnlyList<Guid> tagIds,
        CancellationToken ct = default
    );

    Task<Result> UpdateTagLinksMetadataAsync(
        IReadOnlyDictionary<Guid, string> tagLinkIdToMetadataJson,
        CancellationToken ct = default
    );
}
