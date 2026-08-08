using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Features.ContentTypes.UpdateContentType;

public class UpdateContentTypeHandler(ContentDbContext db)
{
    public async Task<Result<ContentTypeDto>> HandleAsync(
        UpdateContentTypeCommand request,
        CancellationToken cancellationToken)
    {
        var item = await db.ContentTypes.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (item is null) return Result.Fail(new NotFoundError("ContentType not found"));
        
        item.Update(
            request.Name,
            request.DisplayName,
            request.Description,
            request.Icon,
            request.Color,
            request.SortOrder,
            request.FieldsConfig,
            request.DisplayConfig
        );
        
        await db.SaveChangesAsync(cancellationToken);
        
        return Result.Ok(new ContentTypeDto(
            item.Id,
            item.ProjectId,
            item.Key,
            item.Name,
            item.DisplayName,
            item.Description,
            item.Icon,
            item.Color,
            item.SortOrder,
            item.FieldsConfig,
            item.DisplayConfig
        ));
    }
}
