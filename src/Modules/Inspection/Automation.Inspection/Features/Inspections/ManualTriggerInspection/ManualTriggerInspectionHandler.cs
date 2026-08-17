using Automation.Inspection.Features.Inspections.TriggerInspection;
using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspections.ManualTriggerInspection;

[NonTransactional]
public class ManualTriggerInspectionHandler(InspectionDbContext db, IMessageBus bus)
{
    public async Task<Result<IReadOnlyList<InspectionDto>>> HandleAsync(
        ManualTriggerInspectionCommand command,
        CancellationToken ct = default
    )
    {
        if (command.ResourceVersionIds.Count == 0)
            return Result.Fail("No resource versions provided.");

        var inspector = await db.Inspectors
            .AsNoTracking()
            .Include(i => i.Versions)
            .FirstOrDefaultAsync(i => i.Id == command.InspectorId, ct);

        if (inspector == null)
            return Result.Fail($"Inspector with ID '{command.InspectorId}' not found.");

        var publishedVersion = inspector.Versions.FirstOrDefault(v => v.IsPublished);
        if (publishedVersion == null)
            return Result.Fail($"No published version found for inspector '{inspector.Name}'.");

        var runs = command.ResourceVersionIds
            .Select(rvId => new InspectionRun(rvId, publishedVersion.Id, inspector.ExecutorKey))
            .ToList();

        var triggerCommand = new TriggerInspectionCommand(command.AgentId, runs);
        return await bus.InvokeAsync<Result<IReadOnlyList<InspectionDto>>>(triggerCommand, ct);
    }
}
