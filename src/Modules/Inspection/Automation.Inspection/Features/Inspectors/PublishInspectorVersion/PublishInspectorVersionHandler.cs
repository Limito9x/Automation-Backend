using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspectors.PublishInspectorVersion;

[Transactional(typeof(InspectionDbContext))]
public class PublishInspectorVersionHandler(InspectionDbContext db)
{
    public async Task<Result<InspectorVersionDto>> HandleAsync(PublishInspectorVersionCommand command, CancellationToken ct)
    {
        var version = await db.InspectorVersions
            .FirstOrDefaultAsync(x => x.Id == command.VersionId, ct);

        if (version is null)
            return Result.Fail($"Inspector version with ID '{command.VersionId}' was not found.");

        version.SetPublished(command.IsPublished);
        await db.SaveChangesAsync(ct);

        var dto = version.Adapt<InspectorVersionDto>();
        return Result.Ok(dto);
    }
}
