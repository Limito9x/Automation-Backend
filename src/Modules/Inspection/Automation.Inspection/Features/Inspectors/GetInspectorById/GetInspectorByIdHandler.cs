using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspectors.GetInspectorById;

[NonTransactional]
public class GetInspectorByIdHandler(InspectionDbContext db)
{
    public async Task<Result<InspectorDto>> HandleAsync(GetInspectorByIdQuery query, CancellationToken ct)
    {
        var inspector = await db.Inspectors
            .AsNoTracking()
            .Include(x => x.Versions)
            .Where(x => x.Id == query.Id)
            .ProjectToType<InspectorDto>()
            .FirstOrDefaultAsync(ct);

        if (inspector is null)
            return Result.Fail($"Inspector with ID '{query.Id}' was not found.");

        return Result.Ok(inspector);
    }
}
