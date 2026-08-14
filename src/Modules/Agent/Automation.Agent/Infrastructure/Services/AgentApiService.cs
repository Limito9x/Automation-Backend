using Automation.Agent.Contracts;
using Automation.Agent.Features.Connections;
using Automation.Agent.Grpc;
using Automation.Agent.Infrastructure.Persistence;
using FluentResults;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Automation.Agent.Infrastructure.Services;

public class AgentApiService(
    AgentDbContext db,
    IAgentConnectionRegistry registry,
    ICommandTracker commandTracker) : IAgentApi
{
    public async Task<Result<AgentDto>> GetAgentByIdAsync(Guid agentId, CancellationToken ct = default)
    {
        var agent = await db.Agents
            .AsNoTracking()
            .Where(a => a.Id == agentId)
            .ProjectToType<AgentDto>()
            .FirstOrDefaultAsync(ct);

        if (agent is null)
            return Result.Fail($"Agent with ID '{agentId}' was not found.");

        return Result.Ok(agent);
    }

    public async Task<Result<IReadOnlyList<AgentDto>>> GetAgentsByIdsAsync(IEnumerable<Guid> agentIds, CancellationToken ct = default)
    {
        var ids = agentIds.Distinct().ToList();
        if (ids.Count == 0)
            return Result.Ok<IReadOnlyList<AgentDto>>([]);

        var agents = await db.Agents
            .AsNoTracking()
            .Where(a => ids.Contains(a.Id))
            .ProjectToType<AgentDto>()
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<AgentDto>>(agents);
    }

    public async Task<Result<IReadOnlyDictionary<Guid, AgentDto>>> GetAgentsMapByIdsAsync(IEnumerable<Guid> agentIds, CancellationToken ct = default)
    {
        var result = await GetAgentsByIdsAsync(agentIds, ct);
        if (result.IsFailed)
            return result.ToResult();

        var map = result.Value.ToDictionary(a => a.Id);
        return Result.Ok<IReadOnlyDictionary<Guid, AgentDto>>(map);
    }

    public async Task<Result<AgentScanResultDto>> SendScanCommandAsync(
        Guid agentId,
        string directoryPath,
        IEnumerable<string>? extensions = null,
        CancellationToken ct = default)
    {
        if (!registry.TryGet(agentId, out var connection) || connection is null)
        {
            return Result.Fail<AgentScanResultDto>($"Agent với ID '{agentId}' chưa kết nối gRPC ngầm.");
        }

        var commandId = Guid.NewGuid().ToString();
        var scanCommand = new ScanCommand
        {
            CommandId = commandId,
            DirectoryPath = string.IsNullOrWhiteSpace(directoryPath) ? "." : directoryPath
        };

        if (extensions is not null)
        {
            scanCommand.Extensions.AddRange(extensions);
        }

        var task = commandTracker.RegisterCommandAsync(commandId, ct);

        try
        {
            await connection.ResponseStream.WriteAsync(new ServerMessage
            {
                ScanCommand = scanCommand
            }, ct);

            var response = await task;

            if (!response.Success)
            {
                return Result.Fail<AgentScanResultDto>($"Lỗi từ Agent: {response.ErrorMessage}");
            }

            var items = response.ScanResult?.Items
                .Select(x => new AgentScanItemDto(x.RelativePath, x.Hash, x.SizeBytes))
                .ToList();

            return Result.Ok(new AgentScanResultDto(commandId, true, null, items));
        }
        catch (OperationCanceledException)
        {
            return Result.Fail<AgentScanResultDto>("Quá thời gian chờ phản hồi từ Agent.");
        }
        catch (Exception ex)
        {
            return Result.Fail<AgentScanResultDto>($"Lỗi khi gửi lệnh scan: {ex.Message}");
        }
    }

    public async Task<Result<AgentBrowseResultDto>> SendBrowseCommandAsync(
        Guid agentId,
        string directoryPath,
        CancellationToken ct = default)
    {
        if (!registry.TryGet(agentId, out var connection) || connection is null)
        {
            return Result.Fail<AgentBrowseResultDto>($"Agent với ID '{agentId}' chưa kết nối gRPC ngầm.");
        }

        var commandId = Guid.NewGuid().ToString();
        var browseCommand = new BrowseCommand
        {
            CommandId = commandId,
            DirectoryPath = directoryPath ?? string.Empty
        };

        var task = commandTracker.RegisterCommandAsync(commandId, ct);

        try
        {
            await connection.ResponseStream.WriteAsync(new ServerMessage
            {
                BrowseCommand = browseCommand
            }, ct);

            var response = await task;

            if (!response.Success)
            {
                return Result.Fail<AgentBrowseResultDto>($"Lỗi từ Agent: {response.ErrorMessage}");
            }

            var browseResult = response.BrowseResult;
            var currentPath = browseResult?.CurrentPath ?? string.Empty;
            var parentPath = browseResult?.ParentPath ?? string.Empty;
            var canNavigateUp = browseResult?.CanNavigateUp ?? false;

            var items = browseResult?.Items
                .Select(x => new AgentBrowseItemDto(x.Name, x.RelativePath, x.IsDirectory, x.SizeBytes))
                .ToList();

            return Result.Ok(new AgentBrowseResultDto(commandId, true, null, currentPath, parentPath, canNavigateUp, items));
        }
        catch (OperationCanceledException)
        {
            return Result.Fail<AgentBrowseResultDto>("Quá thời gian chờ phản hồi từ Agent.");
        }
        catch (Exception ex)
        {
            return Result.Fail<AgentBrowseResultDto>($"Lỗi khi gửi lệnh browse: {ex.Message}");
        }
    }
}
