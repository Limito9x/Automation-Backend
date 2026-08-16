using Automation.Workspace.Infrastructure.Persistence;
using Automation.Workspace.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Workspace.Features.Resources.GetResourceById;

[NonTransactional]
public class GetResourceByIdHandler(WorkspaceDbContext db)
{
    public async Task<Result<ResourceItemDto>> HandleAsync(GetResourceByIdQuery query, CancellationToken ct)
    {
        var resource = await db.ResourceItems
            .AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Include(x => x.Versions)
            .ProjectToType<ResourceItemDto>()
            .FirstOrDefaultAsync(ct);

        if (resource is null)
            return Result.Fail($"Resource with ID '{query.Id}' was not found.");

        return Result.Ok(resource);
    }
}
