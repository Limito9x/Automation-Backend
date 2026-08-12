using Automation.Agent.Shared.Dtos;

namespace Automation.Agent.Features.Agents.ConfigureAgentPlatform;

public class ConfigureAgentPlatformEndpoint(IMessageBus bus) : Endpoint<ConfigureAgentPlatformCommand, AgentPlatformConfigDto>
{
    public override void Configure()
    {
        Post("/{agentId:guid}/platforms");
        Group<AgentsGroup>();
        Permissions(P.Agent.Update);
    }

    public override async Task HandleAsync(ConfigureAgentPlatformCommand req, CancellationToken ct)
    {
        var agentId = Route<Guid>("agentId");
        var command = req with { AgentId = agentId };
        var result = await bus.InvokeAsync<Result<AgentPlatformConfigDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}

