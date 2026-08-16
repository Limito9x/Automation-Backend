using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspections.GetInspectionsByResourceVersion;

[NonTransactional]
public class GetInspectionsByResourceVersionHandler(InspectionDbContext db)
{
    public async Task<Result<IReadOnlyList<InspectionDto>>> HandleAsync(GetInspectionsByResourceVersionQuery query, CancellationToken ct)
    {
        var inspections = await db.Inspections
            .AsNoTracking()
            .Where(x => x.ResourceVersionId == query.ResourceVersionId)
            .Include(x => x.InspectorVersion)
                .ThenInclude(v => v.Inspector)
            .OrderByDescending(x => x.CreatedAt)
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
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<InspectionDto>>(inspections);
    }
}
