using Automation.Platform.Infrastructure.Persistence;
using Automation.Platform.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Platform.Features.PlatformExtensions.CreateExtension;

public class CreateExtensionHandler(PlatformDbContext db)
{
    public async Task<Result<PlatformExtensionDto>> HandleAsync(CreateExtensionCommand command, CancellationToken ct)
    {
        var rawLower = command.Extension.Trim().ToLowerInvariant();
        var extensionLower = rawLower.StartsWith('.') ? rawLower : "." + rawLower;
        var exists = await db.PlatformExtensions.AnyAsync(
            x => x.Extension == extensionLower, ct);

        if (exists)
            return Result.Fail($"Extension '{extensionLower}' already exists for this platform.");

        var ext = new Domain.Entities.PlatformExtension(extensionLower);
        db.PlatformExtensions.Add(ext);
        await db.SaveChangesAsync(ct);

        return Result.Ok(ext.Adapt<PlatformExtensionDto>());
    }
}

