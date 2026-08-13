using Automation.Agent.Features.Agents;
using Automation.Agent.Grpc;
using FastEndpoints;
using FluentResults;

namespace Automation.Agent.Features.Connections.TestSendMessage;

public record TestScanRequest(
    Guid Id,
    string DirectoryPath,
    List<string>? Extensions
);

public record ResourceItemDto(
    string RelativePath,
    string Hash,
    long SizeBytes
);

public record TestScanResponse(
    bool Success,
    string Message,
    List<ResourceItemDto>? Items
);

public class TestScanEndpoint(
    IAgentConnectionRegistry registry,
    ICommandTracker commandTracker)
    : Endpoint<TestScanRequest, TestScanResponse>
{
    public override void Configure()
    {
        Post("{id:guid}/test-scan");
        Group<AgentsGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(TestScanRequest req, CancellationToken ct)
    {
        if (!registry.TryGet(req.Id, out var connection) || connection is null)
        {
            await this.SendResultAsync(Result.Fail<TestScanResponse>($"Agent với ID '{req.Id}' chưa kết nối gRPC ngầm."), ct);
            return;
        }

        var commandId = Guid.NewGuid().ToString();
        var scanCommand = new ScanCommand
        {
            CommandId = commandId,
            DirectoryPath = string.IsNullOrWhiteSpace(req.DirectoryPath) ? "." : req.DirectoryPath
        };

        if (req.Extensions is not null && req.Extensions.Count > 0)
        {
            scanCommand.Extensions.AddRange(req.Extensions);
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
                await this.SendResultAsync(Result.Fail<TestScanResponse>($"Lỗi từ Agent: {response.ErrorMessage}"), ct);
                return;
            }

            var items = response.ScanResult?.Items.Select(x => new ResourceItemDto(x.RelativePath, x.Hash, x.SizeBytes)).ToList();

            await this.SendResultAsync(Result.Ok(new TestScanResponse(true, $"Scan thành công {items?.Count ?? 0} file.", items)), ct);
        }
        catch (OperationCanceledException)
        {
            await this.SendResultAsync(Result.Fail<TestScanResponse>("Quá thời gian chờ phản hồi từ Agent."), ct);
        }
        catch (Exception ex)
        {
            await this.SendResultAsync(Result.Fail<TestScanResponse>($"Lỗi khi gửi lệnh scan: {ex.Message}"), ct);
        }
    }
}
