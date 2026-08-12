using Automation.Resource.Infrastructure.Persistence;
using Automation.Resource.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Resource.Features.Agents.GetMyAgent;

internal class GetMyAgentHandler(ResourceDbContext db)
{
    public async Task<Result<AgentDto>> HandleAsync(GetMyAgentQuery query, CancellationToken ct)
    {
        var agent = await db.Agents
            .AsNoTracking()
            .Where(x => x.Id == query.AgentId)
            .ProjectToType<AgentDto>()
            .FirstOrDefaultAsync(ct);

        if (agent is null)
            return Result.Fail("Agent not found or inactive.");

        return Result.Ok(agent);
    }
}
