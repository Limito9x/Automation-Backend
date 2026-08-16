using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspections.GetInspectionById;

[NonTransactional]
public class GetInspectionByIdHandler(InspectionDbContext db)
{
    public async Task<Result<InspectionDto>> HandleAsync(GetInspectionByIdQuery query, CancellationToken ct)
    {
        var inspection = await db.Inspections
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Include(x => x.InspectorVersion)
                .ThenInclude(v => v.Inspector)
            .Select(x => new InspectionDto(
                x.Id,
                x.ResourceVersionId,
                x.InspectorVersionId,
                x.InspectorVersion.Inspector.Name,
                x.InspectorVersion.Inspector.Key,
                x.InspectorVersion.Version,
                x.InspectorVersion.Inspector.ExecutorKey,
                x.Status,
                x.Data,
                x.ExecutionTimeMs,
                x.SummaryMessage,
                x.InspectedAt,
                x.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);

        if (inspection is null)
            return Result.Fail($"Inspection with ID '{query.Id}' was not found.");

        return Result.Ok(inspection);
    }
}
