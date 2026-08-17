using Automation.Inspection.Domain.Entities;
using Automation.Inspection.Features.Inspections.TriggerInspection;
using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Automation.Workspace.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspections.AutoTriggerInspection;

[NonTransactional]
public class AutoTriggerInspectionHandler(
    InspectionDbContext db,
    IMessageBus bus,
    ILogger<AutoTriggerInspectionHandler> logger
)
{
    public async Task HandleAsync(ResourcesCreatedEvent message, CancellationToken ct)
    {
        if (message.ResourceVersionIds.Count == 0)
            return;

        logger.LogInformation(
            "Received ResourcesCreatedEvent for Agent {AgentId} with {Count} resource versions.",
            message.AgentId,
            message.ResourceVersionIds.Count
        );

        // 1. Tìm các InspectorRule của Project này đang Enabled (và lấy version đã Published của Inspector)
        var activeRules = await db
            .InspectorRules.AsNoTracking()
            .Where(r => r.ProjectId == message.ProjectId && r.Enabled)
            .Include(r => r.Inspector)
                .ThenInclude(i => i.Versions)
            .ToListAsync(ct);

        if (activeRules.Count == 0)
            return;

        // 2. Ghép cặp ResourceVersionId với các Inspector Version hợp lệ
        var runs = new List<InspectionRun>();

        foreach (var rule in activeRules)
        {
            var publishedVersion = rule.Inspector.Versions.FirstOrDefault(v => v.IsPublished);
            if (publishedVersion is null)
                continue;

            foreach (var resourceVersionId in message.ResourceVersionIds)
            {
                runs.Add(
                    new InspectionRun(
                        resourceVersionId,
                        publishedVersion.Id,
                        rule.Inspector.ExecutorKey
                    )
                );
            }
        }

        if (runs.Count == 0)
            return;

        // 3. Gọi TriggerInspectionCommand để tạo bản ghi và đẩy task vào hàng đợi
        var triggerCommand = new TriggerInspectionCommand(message.AgentId, runs);
        var result = await bus.InvokeAsync<Result<IReadOnlyList<InspectionDto>>>(
            triggerCommand,
            ct
        );

        if (result.IsFailed)
        {
            logger.LogError(
                "Failed to auto trigger inspections: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Message))
            );
        }
    }
}
