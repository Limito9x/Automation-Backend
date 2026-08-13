using FluentResults;

namespace Automation.Agent.Contracts;

public interface IAgentApi
{
    Task<Result<AgentDto>> GetAgentByIdAsync(Guid agentId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<AgentDto>>> GetAgentsByIdsAsync(IEnumerable<Guid> agentIds, CancellationToken ct = default);
    Task<Result<IReadOnlyDictionary<Guid, AgentDto>>> GetAgentsMapByIdsAsync(IEnumerable<Guid> agentIds, CancellationToken ct = default);
    Task<Result<AgentScanResultDto>> SendScanCommandAsync(Guid agentId, string directoryPath, IEnumerable<string>? extensions = null, CancellationToken ct = default);
    Task<Result<AgentBrowseResultDto>> SendBrowseCommandAsync(Guid agentId, string directoryPath, CancellationToken ct = default);
}
