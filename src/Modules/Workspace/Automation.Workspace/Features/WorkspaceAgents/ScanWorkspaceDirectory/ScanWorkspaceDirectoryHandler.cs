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
    public async Task<Result<BrowseDirectoryResultDto>> HandleAsync(
        ScanWorkspaceDirectoryQuery query,
        CancellationToken ct
    )
    {
        var workspaceAgent = await db.WorkspaceAgents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.WorkspaceId == query.WorkspaceId && x.AgentId == query.AgentId, ct);

        // Nếu query.RelativePath được truyền lên (kể cả rỗng ""), sử dụng trực tiếp.
        // Chỉ fallback về workspaceAgent.RootPath khi RelativePath là null hoàn toàn.
        var targetDirectory = query.RelativePath != null
            ? query.RelativePath.Trim()
            : (workspaceAgent?.RootPath?.Trim() ?? string.Empty);

        // Send Browse Folder command to Agent (Only returns directories, no files/hashes)
        var browseResult = await agentApi.SendBrowseCommandAsync(query.AgentId, targetDirectory, ct);
        if (browseResult.IsFailed)
        {
            return Result.Fail<BrowseDirectoryResultDto>(browseResult.Errors);
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

        var resultDto = new BrowseDirectoryResultDto(
            CurrentPath: browseResult.Value.CurrentPath,
            ParentPath: browseResult.Value.ParentPath,
            CanNavigateUp: browseResult.Value.CanNavigateUp,
            Items: nodes
        );

        return Result.Ok(resultDto);
    }
}
