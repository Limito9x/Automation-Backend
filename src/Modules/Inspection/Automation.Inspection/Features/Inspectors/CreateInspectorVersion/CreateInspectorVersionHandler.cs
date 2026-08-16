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
    public async Task<Result<InspectorVersionDto>> HandleAsync(CreateInspectorVersionCommand command, CancellationToken ct)
    {
        var inspector = await db.Inspectors
            .FirstOrDefaultAsync(x => x.Id == command.InspectorId, ct);

        if (inspector is null)
            return Result.Fail($"Inspector with ID '{command.InspectorId}' was not found.");

        var versionTrimmed = command.Version.Trim();
        var exists = await db.InspectorVersions
            .AnyAsync(x => x.InspectorId == command.InspectorId && x.Version == versionTrimmed, ct);

        if (exists)
            return Result.Fail($"Version '{command.Version}' already exists for this Inspector.");

        var inspectorVersion = new InspectorVersion(
            command.InspectorId,
            versionTrimmed,
            command.EntryPoint.Trim(),
            command.ScriptHash.Trim(),
            command.IsPublished
        );

        db.InspectorVersions.Add(inspectorVersion);
        await db.SaveChangesAsync(ct);

        // Verify and Link script asset via Files module
        var linkResult = await assetApi.VerifyAndLinkAsync(
            command.AssetId,
            "InspectorVersion",
            InspectionAssetSlots.Script,
            inspectorVersion.Id.ToString(),
            command.OriginalFileName ?? "script.py",
            0,
            ct
        );

        if (linkResult.IsFailed)
        {
            db.InspectorVersions.Remove(inspectorVersion);
            await db.SaveChangesAsync(ct);
            return Result.Fail($"Failed to link script asset: {string.Join(", ", linkResult.Errors.Select(e => e.Message))}");
        }

        var dto = inspectorVersion.Adapt<InspectorVersionDto>();
        return Result.Ok(dto);
    }
}
