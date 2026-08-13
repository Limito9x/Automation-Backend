using Automation.Agent.Contracts;
using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Workspace.Features.Workspaces.GetWorkspaceById;

[NonTransactional]
public class GetWorkspaceByIdHandler(WorkspaceDbContext db, IAgentApi agentApi)
{
    public async Task<Result<WorkspaceDetailDto>> HandleAsync(GetWorkspaceByIdQuery query, CancellationToken ct)
    {
        var workspaceData = await db.Workspaces
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(x => new
            {
                x.Id,
                x.ProjectId,
                x.Name,
                x.CreatedAt,
                Agents = x.WorkspaceAgents.Select(wa => new
                {
                    wa.Id,
                    wa.AgentId,
                    wa.RootPath,
                    wa.CreatedAt
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (workspaceData is null)
            return Result.Fail($"Workspace with ID '{query.Id}' was not found.");

        var agentIds = workspaceData.Agents.Select(x => x.AgentId).Distinct().ToList();

        IReadOnlyDictionary<Guid, AgentDto> agentMap = new Dictionary<Guid, AgentDto>();
        if (agentIds.Count > 0)
        {
            var agentMapResult = await agentApi.GetAgentsMapByIdsAsync(agentIds, ct);
            if (agentMapResult.IsSuccess)
            {
                agentMap = agentMapResult.Value;
            }
        }

        var workspaceAgents = workspaceData.Agents.Select(wa => new WorkspaceAgentDto(
            wa.Id,
            wa.AgentId,
            wa.RootPath,
            wa.CreatedAt,
            agentMap.GetValueOrDefault(wa.AgentId)
        )).ToList();

        var detailDto = new WorkspaceDetailDto(
            workspaceData.Id,
            workspaceData.ProjectId,
            workspaceData.Name,
            workspaceAgents,
            workspaceData.CreatedAt
        );

        return Result.Ok(detailDto);
    }
}
