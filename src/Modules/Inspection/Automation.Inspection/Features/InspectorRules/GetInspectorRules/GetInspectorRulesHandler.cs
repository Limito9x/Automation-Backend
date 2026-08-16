using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.InspectorRules.GetInspectorRules;

[NonTransactional]
public class GetInspectorRulesHandler(InspectionDbContext db)
{
    public async Task<Result<IReadOnlyList<InspectorRuleDto>>> HandleAsync(GetInspectorRulesQuery query, CancellationToken ct)
    {
        var rules = await db.InspectorRules
            .AsNoTracking()
            .Where(x => x.ProjectId == query.ProjectId)
            .Include(x => x.Inspector)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new InspectorRuleDto(
                x.Id,
                x.ProjectId,
                x.PlatformExtensionId,
                x.ContentTypeId,
                x.InspectorId,
                x.Inspector.Name,
                x.Inspector.Key,
                x.Inspector.ExecutorKey,
                x.Enabled,
                x.CreatedAt
            ))
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<InspectorRuleDto>>(rules);
    }
}
