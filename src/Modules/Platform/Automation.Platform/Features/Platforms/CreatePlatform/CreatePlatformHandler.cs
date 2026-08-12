using Automation.Platform.Features.PlatformExtensions.CreateExtensions;
using Automation.Platform.Infrastructure.Persistence;
using Automation.Platform.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Platform.Features.Platforms.CreatePlatform;

internal class CreatePlatformHandler(PlatformDbContext db)
{
    public async Task<Result<PlatformDto>> HandleAsync(CreatePlatformCommand command, CancellationToken ct)
    {
        var keyExists = await db.Platforms.AnyAsync(x => x.Key == command.Key, ct);
        if (keyExists)
            return Result.Fail($"Platform with Key '{command.Key}' already exists.");

        var platform = new Domain.Entities.Platform(command.Key, command.Name);

        if (command.Extensions is not null && command.Extensions.Count > 0)
        {
            var extensionEntities = await CreateExtensionsHandler.EnsureExtensionsExistAsync(db, command.Extensions, ct);
            platform.SetExtensions(extensionEntities);
        }

        db.Platforms.Add(platform);
        await db.SaveChangesAsync(ct);

        var dto = new PlatformDto(
            platform.Id,
            platform.Key,
            platform.Name,
            platform.Extensions.Select(x => x.Extension).ToList(),
            platform.CreatedAt
        );

        return Result.Ok(dto);
    }
}
