using Automation.Files.Contracts;
using Automation.Inspection.Constants;
using Automation.Inspection.Domain.Entities;
using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspectors.CreateInspectorVersion;

[NonTransactional]
public class CreateInspectorVersionHandler(InspectionDbContext db, IAssetApi assetApi)
{
    public async Task<Result<InspectorVersionDto>> HandleAsync(
        CreateInspectorVersionCommand command,
        CancellationToken ct
    )
    {
        var inspectorExists = await db.Inspectors.AnyAsync(x => x.Id == command.InspectorId, ct);
        if (!inspectorExists)
            return Result.Fail($"Inspector with ID '{command.InspectorId}' was not found.");

        var existingVersions = await db.InspectorVersions
            .Where(x => x.InspectorId == command.InspectorId)
            .ToListAsync(ct);

        var nextVersionNumber = (existingVersions.Count > 0 ? existingVersions.Max(v => v.Version) : 0) + 1;

        if (command.Publish)
        {
            foreach (var old in existingVersions.Where(x => x.IsPublished))
            {
                old.SetPublished(false);
            }
        }

        var newVersion = new InspectorVersion(
            command.InspectorId,
            nextVersionNumber,
            command.EntryPoint,
            command.ScriptHash,
            isPublished: command.Publish
        );

        await db.InspectorVersions.AddAsync(newVersion, ct);
        await db.SaveChangesAsync(ct);

        // Verify and Link script asset via Files module
        if (command.AssetId != Guid.Empty)
        {
            await assetApi.VerifyAndLinkAsync(
                command.AssetId,
                "InspectorVersion",
                InspectionAssetSlots.Script,
                newVersion.Id.ToString(),
                command.EntryPoint,
                0,
                ct
            );
        }

        return newVersion.Adapt<InspectorVersionDto>();
    }
}
