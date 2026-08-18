using Automation.Tag.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.Tag.Features.Tags.DeleteTag;

public class DeleteTagHandler(TagDbContext db)
{
    public async Task<Result> HandleAsync(DeleteTagCommand command, CancellationToken ct)
    {
        var tag = await db.TagItems.FirstOrDefaultAsync(x => x.Id == command.Id, ct);
        if (tag is null)
            return Result.Ok();

        db.TagItems.Remove(tag);
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}