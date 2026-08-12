namespace Automation.Agent.Features.Agents.ConfigureAgentPlatform;

public class ConfigureAgentPlatformValidator : Validator<ConfigureAgentPlatformCommand>
{
    public ConfigureAgentPlatformValidator()
    {
        RuleFor(x => x.AgentId).NotEmpty();
        RuleFor(x => x.PlatformId).NotEmpty();
        RuleFor(x => x.ExecutablePath).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Version).MaximumLength(50);
    }
}

