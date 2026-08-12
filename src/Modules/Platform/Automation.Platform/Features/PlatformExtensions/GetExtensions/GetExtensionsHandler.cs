using Automation.Platform.Infrastructure.Persistence;
using Automation.Platform.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Platform.Features.PlatformExtensions.GetExtensions;

internal class GetExtensionsHandler(PlatformDbContext db)
{
    public async Task<Result<IReadOnlyList<PlatformExtensionDto>>> HandleAsync(GetExtensionsQuery query, CancellationToken ct)
    {
        var extensions = await db.PlatformExtensions
            .AsNoTracking()
            .OrderBy(x => x.Extension)
            .ProjectToType<PlatformExtensionDto>()
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<PlatformExtensionDto>>(extensions);
    }
}
