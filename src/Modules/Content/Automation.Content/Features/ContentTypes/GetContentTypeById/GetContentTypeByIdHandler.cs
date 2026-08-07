using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Features.ContentTypes.GetContentTypeById;

public class GetContentTypeByIdHandler(ContentDbContext db)
{
    public async Task<Result<ContentTypeDto>> HandleAsync(
        GetContentTypeByIdQuery query,
        CancellationToken ct)
    {
        var contentType = await db.ContentTypes.FirstOrDefaultAsync(x => x.Id == query.Id, ct);
        if (contentType is null) return Result.Fail("ContentType not found");
        
        return Result.Ok(new ContentTypeDto(
            contentType.Id,
            contentType.ProjectId,
            contentType.Key,
            contentType.Name,
            contentType.DisplayName,
            contentType.Description,
            contentType.Icon,
            contentType.Color,
            contentType.SortOrder,
            contentType.FieldsConfig,
            contentType.DisplayConfig
        ));
    }
}
