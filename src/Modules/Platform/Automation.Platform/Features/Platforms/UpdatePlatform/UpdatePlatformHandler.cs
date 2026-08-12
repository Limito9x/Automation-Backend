using Automation.Platform.Features.PlatformExtensions.CreateExtensions;
using Automation.Platform.Infrastructure.Persistence;
using Automation.Platform.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Platform.Features.Platforms.UpdatePlatform;

internal class UpdatePlatformHandler(PlatformDbContext db)
{
    public async Task<Result<PlatformDto>> HandleAsync(UpdatePlatformCommand command, CancellationToken ct)
    {
        var platform = await db.Platforms
            .Include(x => x.Extensions)
            .FirstOrDefaultAsync(x => x.Id == command.Id, ct);

        if (platform is null)
            return Result.Fail($"Platform with ID '{command.Id}' was not found.");

        platform.Update(command.Name);

        if (command.Extensions is not null)
        {
            var extensionEntities = await CreateExtensionsHandler.EnsureExtensionsExistAsync(db, command.Extensions, ct);
            platform.SetExtensions(extensionEntities);
        }

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
