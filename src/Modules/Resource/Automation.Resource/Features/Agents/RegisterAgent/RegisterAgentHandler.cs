using System.Security.Cryptography;
using Automation.Resource.Infrastructure.Persistence;
using Automation.Resource.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Resource.Features.Agents.RegisterAgent;

internal class RegisterAgentHandler(ResourceDbContext db)
{
    public async Task<Result<RegisterAgentResultDto>> HandleAsync(RegisterAgentCommand command, CancellationToken ct)
    {
        var existingAgent = await db.Agents
            .FirstOrDefaultAsync(x => x.MachineKey == command.MachineKey, ct);

        if (existingAgent is not null)
        {
            if (!existingAgent.IsActive)
            {
                existingAgent.SetActiveStatus(true);
                await db.SaveChangesAsync(ct);
            }

            return Result.Ok(new RegisterAgentResultDto(
                existingAgent.Id,
                existingAgent.Name,
                existingAgent.MachineKey,
                existingAgent.RegistrationToken
            ));
        }

        var tokenBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        var registrationToken = Convert.ToBase64String(tokenBytes);

        var agent = new Domain.Entities.Agent(command.Name, command.MachineKey, registrationToken);
        db.Agents.Add(agent);
        await db.SaveChangesAsync(ct);

        return Result.Ok(new RegisterAgentResultDto(
            agent.Id,
            agent.Name,
            agent.MachineKey,
            agent.RegistrationToken
        ));
    }
}
