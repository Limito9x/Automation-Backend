using FluentResults;

namespace Automation.Workspace.Contracts;

public interface IWorkspaceApi
{
    Task<Result<ResourceLocationInfoDto>> GetResourceLocationAsync(Guid resourceVersionId, CancellationToken ct = default);
}

public record ResourceLocationInfoDto(
    Guid ResourceVersionId,
    Guid ResourceId,
    string RelativePath,
    string? FileHash,
    Guid? AgentId,
    string? AgentRootPath
)
{
    public string? FullLocalPath
    {
        get
        {
            if (string.IsNullOrWhiteSpace(RelativePath))
                return null;

            if (!string.IsNullOrWhiteSpace(AgentRootPath))
            {
                var cleanRel = RelativePath.TrimStart('/', '\\');
                return Path.Combine(AgentRootPath, cleanRel).Replace('\\', '/');
            }

            return RelativePath;
        }
    }
}
