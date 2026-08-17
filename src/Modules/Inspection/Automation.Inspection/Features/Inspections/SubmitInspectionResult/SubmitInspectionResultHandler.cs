using System.Text.Json;
using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspections.SubmitInspectionResult;

[Transactional(typeof(InspectionDbContext))]
public class SubmitInspectionResultHandler(InspectionDbContext db)
{
    public async Task<Result<InspectionDto>> HandleAsync(
        SubmitInspectionResultCommand command,
        CancellationToken ct
    )
    {
        var inspection = await db
            .Inspections.Include(x => x.InspectorVersion)
                .ThenInclude(v => v.Inspector)
            .FirstOrDefaultAsync(x => x.Id == command.InspectionId, ct);

        if (inspection is null)
            return Result.Fail($"Inspection with ID '{command.InspectionId}' was not found.");

        if (
            command.Data.HasValue
            && command.Data.Value.ValueKind != JsonValueKind.Null
            && command.Data.Value.ValueKind != JsonValueKind.Undefined
        )
        {
            var jsonDoc = JsonDocument.Parse(command.Data.Value.GetRawText());
            inspection.Complete(
                command.Status,
                jsonDoc,
                command.ExecutionTimeMs,
                command.SummaryMessage
            );
        }
        else
        {
            inspection.Fail(
                command.SummaryMessage ?? "Inspection finished without data.",
                command.ExecutionTimeMs
            );
        }

        await db.SaveChangesAsync(ct);

        var dto = new InspectionDto(
            inspection.Id,
            inspection.ResourceVersionId,
            inspection.InspectorVersionId,
            inspection.InspectorVersion?.Inspector?.Name,
            inspection.InspectorVersion?.Inspector?.Key,
            inspection.InspectorVersion?.Version ?? 0,
            inspection.InspectorVersion?.Inspector?.ExecutorKey,
            inspection.Status,
            inspection.Data,
            inspection.ExecutionTimeMs,
            inspection.SummaryMessage,
            inspection.InspectedAt,
            inspection.CreatedAt
        );

        return Result.Ok(dto);
    }
}
