using Automation.Platform.Infrastructure.Persistence;
using Automation.Platform.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Platform.Features.Platforms.GetPlatforms;

internal class GetPlatformsHandler(PlatformDbContext db)
{
    public async Task<Result<IReadOnlyList<PlatformDto>>> HandleAsync(GetPlatformsQuery query, CancellationToken ct)
    {
        var platforms = await db.Platforms
            .AsNoTracking()
            .Include(x => x.Extensions)
            .OrderBy(x => x.Name)
            .Select(x => new PlatformDto(
                x.Id,
                x.Key,
                x.Name,
                x.Extensions.Select(e => e.Extension).ToList(),
                x.CreatedAt
            ))
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<PlatformDto>>(platforms);
    }
}
