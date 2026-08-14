using FluentResults;

namespace Automation.Content.Contracts;

public interface IContentApi
{
    Task<Result<IReadOnlyDictionary<Guid, ContentSummaryDto>>> GetContentsByProjectIdAsync(
        Guid projectId, 
        CancellationToken ct = default);
}
