using Automation.Content.Contracts;
using Automation.SharedKernel.Abstractions.Querying;
using Automation.SharedKernel.Infrastructure.Querying;
using Automation.Workspace.Domain.Entities;
using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using FluentResults;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Workspace.Features.WorkspaceAgents.GetWorkspaceAgentResources;

[NonTransactional]
public class GetWorkspaceAgentResourcesHandler(
    WorkspaceDbContext db,
    IContentApi contentApi
)
{
    public async Task<Result<PagedResult<WorkspaceAgentResourceDto>>> HandleAsync(
        GetWorkspaceAgentResourcesQuery query, 
        CancellationToken ct
    )
    {
        var workspaceAgent = await db.WorkspaceAgents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.WorkspaceId == query.WorkspaceId && x.AgentId == query.AgentId, ct);

        if (workspaceAgent is null)
        {
            return Result.Ok(PagedResult<WorkspaceAgentResourceDto>.From([], 0, query.Page ?? 1, query.PageSize ?? 10));
        }

        var baseQuery = db.ResourceVersionLocations
            .AsNoTracking()
            .Include(l => l.ResourceVersion)
                .ThenInclude(v => v.Resource)
            .Where(l => l.WorkspaceAgentId == workspaceAgent.Id);

        // 1. Lấy Map Content của Project
        var contentMap = new Dictionary<Guid, ContentSummaryDto>();
        var contentResult = await contentApi.GetContentsByProjectIdAsync(query.ProjectId, ct);
        if (contentResult.IsSuccess && contentResult.Value != null)
        {
            contentMap = new Dictionary<Guid, ContentSummaryDto>(contentResult.Value);
        }

        // 2. Omni-Search nếu có từ khóa
        if (!string.IsNullOrWhiteSpace(query.GlobalKeyword))
        {
            var kw = query.GlobalKeyword.Trim();
            var matchedContentIds = contentMap
                .Where(c => c.Value.Name.Contains(kw, StringComparison.OrdinalIgnoreCase) || 
                            c.Value.ContentTypeName.Contains(kw, StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Key)
                .ToList();

            baseQuery = baseQuery.Where(l => 
                l.ResourceVersion.Resource.Name.Contains(kw) || 
                l.RelativePath.Contains(kw) ||
                (l.ResourceVersion.Resource.ContentId != null && matchedContentIds.Contains(l.ResourceVersion.Resource.ContentId.Value)));
        }

        // 3. Gridify phân trang
        var mapper = new GridifyMapper<ResourceVersionLocation>()
            .GenerateMappings()
            .AddMap("resourceName", l => l.ResourceVersion.Resource.Name)
            .AddMap("relativePath", l => l.RelativePath)
            .AddMap("versionNo", l => l.ResourceVersion.VersionNo)
            .AddMap("discoveredAt", l => l.DiscoveredAt);

        var pagedResult = await baseQuery.ToPagedResultAsync(query, mapper, ct);
        if (pagedResult.IsFailed)
        {
            return pagedResult.ToResult();
        }

        // 4. Enrich dữ liệu Content
        var dtos = pagedResult.Value.Items.Select(l => {
            var res = l.ResourceVersion.Resource;
            var content = res.ContentId != null && contentMap.TryGetValue(res.ContentId.Value, out var c) ? c : null;

            return new WorkspaceAgentResourceDto(
                ResourceId: res.Id,
                ResourceName: res.Name,
                RelativePath: l.RelativePath,
                VersionNo: l.ResourceVersion.VersionNo,
                IsOrigin: l.IsOrigin,
                FileHash: l.ResourceVersion.FileHash,
                DiscoveredAt: l.DiscoveredAt,
                ContentId: res.ContentId,
                ContentName: content?.Name,
                ContentTypeName: content?.ContentTypeName,
                ContentTypeColor: content?.ContentTypeColor
            );
        }).ToList();

        return Result.Ok(PagedResult<WorkspaceAgentResourceDto>.From(
            dtos, 
            pagedResult.Value.TotalCount, 
            pagedResult.Value.Page, 
            pagedResult.Value.PageSize
        ));
    }
}
