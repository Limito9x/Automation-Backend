using Automation.Resource.Infrastructure.Persistence;
using Automation.Resource.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Resource.Features.Agents.GetAgents;

internal class GetAgentsHandler(ResourceDbContext db)
{
    public async Task<Result<IReadOnlyList<AgentDto>>> HandleAsync(GetAgentsQuery query, CancellationToken ct)
    {
        var dbQuery = db.Agents.AsNoTracking();

        if (query.IsActive.HasValue)
            dbQuery = dbQuery.Where(x => x.IsActive == query.IsActive.Value);

        var agents = await dbQuery
            .OrderByDescending(x => x.CreatedAt)
            .ProjectToType<AgentDto>()
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<AgentDto>>(agents);
    }
}
