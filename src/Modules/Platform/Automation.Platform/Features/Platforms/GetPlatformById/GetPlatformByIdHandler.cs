using Automation.Platform.Infrastructure.Persistence;
using Automation.Platform.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Platform.Features.Platforms.GetPlatformById;

internal class GetPlatformByIdHandler(PlatformDbContext db)
{
    public async Task<Result<PlatformDto>> HandleAsync(GetPlatformByIdQuery query, CancellationToken ct)
    {
        var platform = await db.Platforms
            .AsNoTracking()
            .Include(x => x.Extensions)
            .Where(x => x.Id == query.Id)
            .Select(x => new PlatformDto(
                x.Id,
                x.Key,
                x.Name,
                x.Extensions.Select(e => e.Extension).ToList(),
                x.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);

        if (platform is null)
            return Result.Fail($"Platform with ID '{query.Id}' was not found.");

        return Result.Ok(platform);
    }
}
