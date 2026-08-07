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
        var contentType = await db.ContentTypes.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (contentType is null) return Result.Fail("ContentType not found");
        
        contentType.Update(
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
