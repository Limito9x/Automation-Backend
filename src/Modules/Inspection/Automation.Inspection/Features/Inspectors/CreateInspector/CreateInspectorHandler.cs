using System.Text.RegularExpressions;
using Automation.Files.Contracts;
using Automation.Inspection.Constants;
using Automation.Inspection.Domain.Entities;
using Automation.Inspection.Infrastructure.Persistence;
using Automation.Inspection.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspectors.CreateInspector;

[NonTransactional]
public class CreateInspectorHandler(InspectionDbContext db, IAssetApi assetApi)
{
    public async Task<Result<InspectorDto>> HandleAsync(
        CreateInspectorCommand command,
        CancellationToken ct
    )
    {
        var baseKey = Slugify(command.Name);
        if (string.IsNullOrWhiteSpace(baseKey))
        {
            baseKey = $"inspector-{Guid.NewGuid():N[..8]}";
        }

        var key = baseKey;
        var suffix = 1;
        while (await db.Inspectors.AnyAsync(x => x.ProjectId == command.ProjectId && x.Key == key, ct))
        {
            key = $"{baseKey}-{suffix++}";
        }

        var inspector = Inspector.Create(
            command.ProjectId,
            key,
            command.Name,
            command.ExecutorKey,
            command.EntryPoint,
            command.ScriptHash,
            command.Description
        );

        db.Inspectors.Add(inspector);
        await db.SaveChangesAsync(ct);

        var publishedVersion = inspector.GetPublishedVersion();
        if (publishedVersion is not null && command.AssetId != Guid.Empty)
        {
            await assetApi.VerifyAndLinkAsync(
                command.AssetId,
                "InspectorVersion",
                InspectionAssetSlots.Script,
                publishedVersion.Id.ToString(),
                command.EntryPoint,
                0,
                ct
            );
        }

        var dto = inspector.Adapt<InspectorDto>();
        return Result.Ok(dto);
    }

    private static string Slugify(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var normalized = text.Trim().ToLowerInvariant();
        var clean = Regex.Replace(normalized, @"[^a-z0-9\-_]+", "-");
        clean = Regex.Replace(clean, @"-+", "-").Trim('-');
        return clean;
    }
}
