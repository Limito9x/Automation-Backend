using Automation.Inspection.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspectors.DeleteInspector;

[Transactional(typeof(InspectionDbContext))]
public class DeleteInspectorHandler(InspectionDbContext db)
{
    public async Task<Result> HandleAsync(DeleteInspectorCommand command, CancellationToken ct)
    {
        var inspector = await db.Inspectors
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

        if (inspector is null)
            return Result.Fail($"Inspector with ID '{command.Id}' was not found.");

        db.Inspectors.Remove(inspector);
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
