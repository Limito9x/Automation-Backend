using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Features.ContentTypes.DeleteContentType;

public class DeleteContentTypeHandler(ContentDbContext db)
{
    public async Task<Result> HandleAsync(
        DeleteContentTypeCommand command,
        CancellationToken ct)
    {
        var contentType = await db.ContentTypes.FirstOrDefaultAsync(x => x.Id == command.Id, ct);
        if (contentType is null) return Result.Fail("ContentType not found");
        
        db.ContentTypes.Remove(contentType);
        await db.SaveChangesAsync(ct);
        
        return Result.Ok();
    }
}
