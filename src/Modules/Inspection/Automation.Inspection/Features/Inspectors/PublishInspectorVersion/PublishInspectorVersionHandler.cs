using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspectors.PublishInspectorVersion;

[Transactional(typeof(InspectionDbContext))]
public class PublishInspectorVersionHandler(InspectionDbContext db)
{
    public async Task<Result<InspectorVersionDto>> HandleAsync(
        PublishInspectorVersionCommand command,
        CancellationToken ct
    )
    {
        var inspector = await db
            .Inspectors.Include(x => x.Versions)
            .Where(x => x.Versions.Any(v => v.Id == command.VersionId))
            .FirstOrDefaultAsync(ct);

        if (inspector is null)
            return Result.Fail($"Inspector with ID '{command.VersionId}' was not found.");

        inspector.SetPublishedVersion(command.VersionId);
        await db.SaveChangesAsync(ct);

        return inspector.GetPublishedVersion()!.Adapt<InspectorVersionDto>();
    }
}
