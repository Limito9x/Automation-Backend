using Automation.Agent.Contracts;
using Automation.Files.Contracts;
using Automation.Inspection.Constants;
using Automation.Inspection.Domain.Entities;
using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Automation.Workspace.Contracts;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspections.TriggerInspection;

[NonTransactional]
public class TriggerInspectionHandler(
    InspectionDbContext db,
    IAssetApi assetApi,
    IAgentApi agentApi,
    IWorkspaceApi workspaceApi)
{
    public async Task<Result<IReadOnlyList<InspectionDto>>> HandleAsync(
        TriggerInspectionCommand command,
        IMessageContext context,
        CancellationToken ct)
    {
        var inspectorsQuery = db.Inspectors
            .AsNoTracking()
            .Where(x => x.ProjectId == command.ProjectId)
            .Include(x => x.Versions.Where(v => v.IsPublished));

        List<Inspector> inspectors;
        if (command.SpecificInspectorId.HasValue)
        {
            var single = await inspectorsQuery
                .FirstOrDefaultAsync(x => x.Id == command.SpecificInspectorId.Value, ct);

            if (single is null)
                return Result.Fail($"Inspector with ID '{command.SpecificInspectorId.Value}' was not found in this project.");

            inspectors = [single];
        }
        else
        {
            inspectors = await inspectorsQuery.ToListAsync(ct);
        }

        if (inspectors.Count == 0)
            return Result.Fail("No active inspectors found for this project.");

        var createdInspections = new List<Domain.Entities.Inspection>();
        var tasksToPublish = new List<InspectResourceTask>();

        foreach (var inspector in inspectors)
        {
            var latestVersion = inspector.Versions
                .OrderByDescending(v => v.CreatedAt)
                .FirstOrDefault();

            if (latestVersion is null)
                continue; // Skip if no published version

            // 1. Get script download URL from Files module
            var filesResult = await assetApi.GetFilesAsync(
                latestVersion.Id.ToString(),
                "InspectorVersion",
                InspectionAssetSlots.Script,
                ct
            );

            var scriptUrl = string.Empty;
            if (filesResult.IsSuccess && filesResult.Value.Count > 0)
            {
                scriptUrl = filesResult.Value[0].PublicUrl;
            }

            // 2. Find matching Agent for the executorKey
            var agentsResult = await agentApi.GetAgentIdsByExecutorKeyAsync(inspector.ExecutorKey, ct);
            var targetAgentId = (agentsResult.IsSuccess && agentsResult.Value.Count > 0)
                ? agentsResult.Value[0]
                : Guid.Empty;

            // 3. Create Inspection record for each ResourceVersion
            foreach (var resourceVersionId in command.ResourceVersionIds)
            {
                var inspection = new Domain.Entities.Inspection(resourceVersionId, latestVersion.Id);
                createdInspections.Add(inspection);
                db.Inspections.Add(inspection);

                // Lấy đường dẫn file vật lý trên Agent từ Workspace Module
                var locResult = await workspaceApi.GetResourceLocationAsync(resourceVersionId, ct);
                var targetFilePath = locResult.IsSuccess ? locResult.Value.FullLocalPath : null;
                var assignedAgentId = (locResult.IsSuccess && locResult.Value.AgentId.HasValue) 
                    ? locResult.Value.AgentId.Value 
                    : targetAgentId;

                tasksToPublish.Add(new InspectResourceTask(
                    inspection.Id,
                    assignedAgentId,
                    resourceVersionId,
                    scriptUrl,
                    latestVersion.ScriptHash,
                    latestVersion.EntryPoint,
                    inspector.ExecutorKey,
                    ResourceFilePath: targetFilePath
                ));
            }
        }

        if (createdInspections.Count == 0)
            return Result.Fail("No published inspector versions found to trigger inspection.");

        await db.SaveChangesAsync(ct);

        // 4. Publish Wolverine tasks (Tu dong gui vao RabbitMQ queue "tasks.inspect" nho routing o Module)
        foreach (var task in tasksToPublish)
        {
            await context.PublishAsync(task);
        }

        var dtos = createdInspections.Select(x => new InspectionDto(
            x.Id,
            x.ResourceVersionId,
            x.InspectorVersionId,
            null,
            null,
            null,
            null,
            x.Status,
            x.Data,
            x.ExecutionTimeMs,
            x.SummaryMessage,
            x.InspectedAt,
            x.CreatedAt
        )).ToList();

        return Result.Ok<IReadOnlyList<InspectionDto>>(dtos);
    }
}
