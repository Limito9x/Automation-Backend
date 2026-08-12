using Automation.Resource.Infrastructure.Persistence;

namespace Automation.Resource.Features.Resources.DeleteResource;

public class DeleteResourceHandler(ResourceDbContext db)
{
    public async Task<Result> HandleAsync(DeleteResourceCommand command, CancellationToken ct)
    {
        var resource = await db.ResourceItems.FindAsync([command.Id], ct);
        if (resource is null)
            return Result.Fail($"Resource with ID '{command.Id}' was not found.");

        db.ResourceItems.Remove(resource);
        await db.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

