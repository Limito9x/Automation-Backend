using Automation.Resource.Infrastructure.Persistence;
using Automation.Resource.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Resource.Features.Resources.GetResourceById;

public class GetResourceByIdHandler(ResourceDbContext db)
{
    public async Task<Result<ResourceItemDto>> HandleAsync(GetResourceByIdQuery query, CancellationToken ct)
    {
        var resource = await db.ResourceItems
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .ProjectToType<ResourceItemDto>()
            .FirstOrDefaultAsync(ct);

        if (resource is null)
            return Result.Fail($"Resource with ID '{query.Id}' was not found.");

        return Result.Ok(resource);
    }
}

