using Automation.Agent.Domain.Entities;
using Automation.Agent.Infrastructure.Persistence;
using Automation.Agent.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Agent.Features.Agents.ConfigureAgentPlatform;

internal class ConfigureAgentPlatformHandler(AgentDbContext db)
{
    public async Task<Result<AgentPlatformConfigDto>> HandleAsync(ConfigureAgentPlatformCommand command, CancellationToken ct)
    {
        var agent = await db.Agents
            .Include(x => x.PlatformConfigs)
            .FirstOrDefaultAsync(x => x.Id == command.AgentId, ct);

        if (agent is null)
            return Result.Fail($"Agent with ID '{command.AgentId}' was not found.");

        var existingConfig = agent.PlatformConfigs
            .FirstOrDefault(x => x.PlatformId == command.PlatformId);

        if (existingConfig is not null)
        {
            existingConfig.Update(command.ExecutablePath, command.Version);
        }
        else
        {
            existingConfig = new AgentPlatformConfig(
                command.AgentId,
                command.PlatformId,
                command.ExecutablePath,
                command.Version
            );
            db.AgentPlatformConfigs.Add(existingConfig);
        }

        await db.SaveChangesAsync(ct);

        var dto = existingConfig.Adapt<AgentPlatformConfigDto>();
        return Result.Ok(dto);
    }
}
