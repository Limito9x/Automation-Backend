using Automation.Platform.Infrastructure.Persistence;

namespace Automation.Platform.Features.Platforms.DeletePlatform;

internal class DeletePlatformHandler(PlatformDbContext db)
{
    public async Task<Result> HandleAsync(DeletePlatformCommand command, CancellationToken ct)
    {
        var platform = await db.Platforms.FindAsync([command.Id], ct);
        if (platform is null)
            return Result.Fail($"Platform with ID '{command.Id}' was not found.");

        db.Platforms.Remove(platform);
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
