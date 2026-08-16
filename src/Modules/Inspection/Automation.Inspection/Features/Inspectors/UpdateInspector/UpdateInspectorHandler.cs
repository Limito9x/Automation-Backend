using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspectors.UpdateInspector;

[Transactional(typeof(InspectionDbContext))]
public class UpdateInspectorHandler(InspectionDbContext db)
{
    public async Task<Result<InspectorDto>> HandleAsync(UpdateInspectorCommand command, CancellationToken ct)
    {
        var inspector = await db.Inspectors
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

        if (inspector is null)
            return Result.Fail($"Inspector with ID '{command.Id}' was not found.");

        inspector.Update(command.Name, command.ExecutorKey, command.Description);
        await db.SaveChangesAsync(ct);

        var dto = inspector.Adapt<InspectorDto>();
        return Result.Ok(dto);
    }
}
