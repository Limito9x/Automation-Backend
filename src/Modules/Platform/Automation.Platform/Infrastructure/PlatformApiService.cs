using Automation.Platform.Contracts;
using Automation.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.Platform.Infrastructure;

internal class PlatformApiService(PlatformDbContext db) : IPlatformApi
{
    public async Task<Result<IReadOnlyList<string>>> GetAllowedExtensionsAsync(Guid platformId, CancellationToken ct = default)
    {
        var platform = await db.Platforms
            .AsNoTracking()
            .Include(x => x.Extensions)
            .FirstOrDefaultAsync(x => x.Id == platformId, ct);

        if (platform is null)
            return Result.Ok<IReadOnlyList<string>>([]);

        var extensions = platform.Extensions
            .Select(x => x.Extension.ToLowerInvariant())
            .ToList();

        return Result.Ok<IReadOnlyList<string>>(extensions);
    }

    public async Task<Result<Guid?>> GetExtensionIdAsync(Guid platformId, string extension, CancellationToken ct = default)
    {
        var extLower = extension.ToLowerInvariant();
        var extEntity = await db.PlatformExtensions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Extension == extLower, ct);

        return Result.Ok(extEntity?.Id);
    }
}
