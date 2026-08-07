using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;

namespace Automation.Content.Features.ContentTypes.CreateContentType;

public class CreateContentTypeHandler(ContentDbContext db)
{
    public async Task<Result<ContentTypeDto>> HandleAsync(
        CreateContentTypeCommand request,
        CancellationToken cancellationToken)
    {
        var contentType = new ContentType(
            request.ProjectId,
            request.Key,
            request.Name,
            request.DisplayName,
            request.Description,
            request.Icon,
            request.Color,
            request.SortOrder,
            request.FieldsConfig,
            request.DisplayConfig
        );
        
        db.ContentTypes.Add(contentType);
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
