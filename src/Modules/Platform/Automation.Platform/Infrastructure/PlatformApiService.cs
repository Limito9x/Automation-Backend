using Automation.Platform.Contracts;
using Automation.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.Platform.Infrastructure;

public class PlatformApiService(PlatformDbContext db) : IPlatformApi
{
    public async Task<Result<IReadOnlyList<string>>> GetAllowedExtensionsAsync(
        Guid platformId,
        CancellationToken ct = default
    )
    {
        var platform = await db
            .Platforms.AsNoTracking()
            .Include(x => x.Extensions)
            .FirstOrDefaultAsync(x => x.Id == platformId, ct);

        if (platform is null)
            return Result.Ok<IReadOnlyList<string>>([]);

        var extensions = platform.Extensions.Select(x => x.Extension.ToLowerInvariant()).ToList();

        return Result.Ok<IReadOnlyList<string>>(extensions);
    }

    public async Task<Result<IReadOnlyList<string>>> GetAllowedExtensionsAsync(
        IEnumerable<Guid> platformIds,
        CancellationToken ct = default
    )
    {
        var ids = platformIds.Distinct().Select(x => x.ToString()).ToList();

        if (ids.Count == 0)
            return Result.Ok<IReadOnlyList<string>>([]);

        var extensions = db
            .Platforms.AsNoTracking()
            .Where(p => ids.Contains(p.Id.ToString()))
            .SelectMany(p => p.Extensions)
            .Select(x => x.Extension.ToLower())
            .Distinct();

        return Result.Ok<IReadOnlyList<string>>(await extensions.ToListAsync(ct));
    }

    public async Task<Result<Guid?>> GetExtensionIdAsync(
        Guid platformId,
        string extension,
        CancellationToken ct = default
    )
    {
        var extLower = extension.ToLowerInvariant();
        var extEntity = await db
            .PlatformExtensions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Extension == extLower, ct);

        return Result.Ok(extEntity?.Id);
    }

    public async Task<Result<Dictionary<string, Guid>>> GetExtensionMapAsync(
        IEnumerable<Guid>? platformIds,
        CancellationToken ct = default
    )
    {
        var extensions = db
            .PlatformExtensions.AsNoTracking()
            .Select(x => new { x.Extension, x.Id });

        if (platformIds != null && platformIds.Any())
        {
            var ids = platformIds.Distinct().Select(x => x.ToString()).ToList();
            extensions = extensions.Where(x => ids.Contains(x.Id.ToString()));
        }

        var map = await extensions.ToDictionaryAsync(x => x.Extension.ToLower(), x => x.Id, ct);

        return Result.Ok(map);
    }
}
