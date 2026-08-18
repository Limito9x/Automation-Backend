using Automation.Files.Contracts;
using Automation.Inspection.Constants;
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
        var inspector = await db
            .Inspectors.Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == command.InspectorId, ct);

        if (inspector is null)
            return Result.Fail($"Inspector with ID '{command.InspectorId}' was not found.");

        inspector.AddNewVersion(command.EntryPoint, command.ScriptHash, command.Publish);
        await db.SaveChangesAsync(ct);

        // Verify and Link script asset via Files module
        var linkResult = await assetApi.VerifyAndLinkAsync(
            command.AssetId,
            "InspectorVersion",
            InspectionAssetSlots.Script,
            inspector.GetPublishedVersion()!.Id.ToString(),
            command.EntryPoint,
            0,
            ct
        );

        return inspector.GetPublishedVersion()!.Adapt<InspectorVersionDto>();
    }
}
