using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspectors.GetInspectors;

[NonTransactional]
public class GetInspectorsHandler(InspectionDbContext db)
{
    public async Task<Result<IReadOnlyList<InspectorDto>>> HandleAsync(GetInspectorsQuery query, CancellationToken ct)
    {
        var inspectors = await db.Inspectors
            .AsNoTracking()
            .Where(x => x.ProjectId == query.ProjectId)
            .Include(x => x.Versions)
            .OrderByDescending(x => x.CreatedAt)
            .ProjectToType<InspectorDto>()
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<InspectorDto>>(inspectors);
    }
}
