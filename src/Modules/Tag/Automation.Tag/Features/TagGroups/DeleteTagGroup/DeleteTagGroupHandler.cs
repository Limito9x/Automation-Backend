using Automation.Tag.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.Tag.Features.TagGroups.DeleteTagGroup;

public class DeleteTagGroupHandler(TagDbContext db)
{
    public async Task<Result> HandleAsync(DeleteTagGroupCommand command, CancellationToken ct)
    {
        var group = await db.TagGroups.FirstOrDefaultAsync(x => x.Id == command.Id, ct);
        if (group is null)
            return Result.Ok(); // Idempotent

        db.TagGroups.Remove(group);
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}