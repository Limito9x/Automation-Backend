using Automation.Files.Contracts;
using Automation.Inspection.Constants;
using Automation.Inspection.Domain.Entities;
using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Automation.Workspace.Contracts;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspections.TriggerInspection;

[NonTransactional]
public class TriggerInspectionHandler(
    InspectionDbContext db,
    IAssetApi assetApi,
    IWorkspaceApi workspaceApi
)
{
    public async Task<Result<IReadOnlyList<InspectionDto>>> HandleAsync(
        TriggerInspectionCommand command,
        IMessageContext context,
        CancellationToken ct
    )
    {
        if (command.Runs.Count == 0)
            return Result.Fail("No inspection runs provided.");

        // 1. Load all referenced inspector versions in a single query
        var versionIds = command.Runs.Select(r => r.InspectorVersionId).Distinct().ToList();
        var versions = await db
            .InspectorVersions.AsNoTracking()
            .Where(v => versionIds.Contains(v.Id))
            .Include(v => v.Inspector)
            .ToListAsync(ct);

        var versionLookup = versions.ToDictionary(v => v.Id);

        foreach (var run in command.Runs)
        {
            if (!versionLookup.TryGetValue(run.InspectorVersionId, out var version))
                return Result.Fail(
                    $"Inspector version with ID '{run.InspectorVersionId}' was not found."
                );
        }

        // 2. Create a Pending Inspection for each run and build the corresponding task
        var createdInspections = new List<Domain.Entities.Inspection>();
        var tasksToPublish = new List<InspectResourceTask>();

        var assetDictionary = await assetApi.GetFilesAsync(
            command.Runs.Select(r => r.InspectorVersionId.ToString()),
            "InspectorVersion",
            InspectionAssetSlots.Script,
            ct
        );

        if (!assetDictionary.IsSuccess)
            return Result.Fail("Failed to get inspector script files.");

        var resourceLocations = await workspaceApi.GetResourceLocationsAsync(
            command.Runs.Select(r => r.ResourceVersionId),
            command.AgentId,
            ct
        );

        if (!resourceLocations.IsSuccess)
            return Result.Fail("Failed to get resource locations.");

        foreach (var run in command.Runs)
        {
            var version = versionLookup[run.InspectorVersionId];

            // Get script download URL from Files module
            if (
                !assetDictionary.Value.TryGetValue(run.InspectorVersionId.ToString(), out var files)
            )
                continue;

            var scriptUrl = files.Count > 0 ? files[0].PublicUrl : string.Empty;

            // Get the physical file path on the Agent from Workspace module
            var locResult = resourceLocations.Value[run.ResourceVersionId.ToString()];
            var targetFilePath = locResult.FullLocalPath;

            var inspection = new Domain.Entities.Inspection(
                run.ResourceVersionId,
                run.InspectorVersionId
            );
            createdInspections.Add(inspection);
            db.Inspections.Add(inspection);

            tasksToPublish.Add(
                new InspectResourceTask(
                    inspection.Id,
                    command.AgentId,
                    run.ResourceVersionId,
                    scriptUrl,
                    version.ScriptHash,
                    version.EntryPoint,
                    run.ExecutorKey,
                    ResourceFilePath: targetFilePath
                )
            );
        }

        await db.SaveChangesAsync(ct);

        // 3. Publish one message per task to RabbitMQ queue "tasks.inspect"
        foreach (var task in tasksToPublish)
        {
            await context.PublishAsync(task);
        }

        var dtos = createdInspections
            .Select(x =>
            {
                var version = versionLookup[x.InspectorVersionId];
                return new InspectionDto(
                    x.Id,
                    x.ResourceVersionId,
                    x.InspectorVersionId,
                    version.Inspector.Name,
                    version.Inspector.Key,
                    version.Version,
                    version.Inspector.ExecutorKey,
                    x.Status,
                    x.Data,
                    x.ExecutionTimeMs,
                    x.SummaryMessage,
                    x.InspectedAt,
                    x.CreatedAt
                );
            })
            .ToList();

        return Result.Ok<IReadOnlyList<InspectionDto>>(dtos);
    }
}
