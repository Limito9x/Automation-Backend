using Automation.Agent.Contracts;
using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Workspace.Features.WorkspaceAgents.ScanWorkspaceDirectory;

[NonTransactional]
public class ScanWorkspaceDirectoryHandler(
    WorkspaceDbContext db,
    IAgentApi agentApi
)
{
    public async Task<Result<IReadOnlyList<DirectoryNodeDto>>> HandleAsync(
        ScanWorkspaceDirectoryQuery query,
        CancellationToken ct
    )
    {
        var workspaceAgent = await db.WorkspaceAgents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.WorkspaceId == query.WorkspaceId && x.AgentId == query.AgentId, ct);

        var relativePath = query.RelativePath?.Trim() ?? string.Empty;

        string targetDirectory;
        if (!string.IsNullOrWhiteSpace(relativePath))
        {
            targetDirectory = relativePath;
        }
        else if (workspaceAgent is not null && !string.IsNullOrWhiteSpace(workspaceAgent.RootPath))
        {
            targetDirectory = workspaceAgent.RootPath;
        }
        else
        {
            // Empty string indicates Agent should return System Drives / Root System folders
            targetDirectory = "";
        }

        // Send Browse Folder command to Agent (Only returns directories, no files/hashes)
        var browseResult = await agentApi.SendBrowseCommandAsync(query.AgentId, targetDirectory, ct);
        if (browseResult.IsFailed)
        {
            return Result.Fail<IReadOnlyList<DirectoryNodeDto>>(browseResult.Errors);
        }

        var items = browseResult.Value.Items ?? [];
        
        // Filter ONLY directories for folder selection browser
        var nodes = items
            .Where(item => item.IsDirectory)
            .Select(item => new DirectoryNodeDto(
                Name: item.Name,
                Path: item.RelativePath.Replace('\\', '/'),
                IsDirectory: true,
                SizeBytes: 0,
                HasChildren: true
            ))
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Result.Ok<IReadOnlyList<DirectoryNodeDto>>(nodes);
    }
}
