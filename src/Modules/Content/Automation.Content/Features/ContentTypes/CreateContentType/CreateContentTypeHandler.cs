using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Automation.DynamicForms.Contracts;

namespace Automation.Content.Features.ContentTypes.CreateContentType;

public class CreateContentTypeHandler(ContentDbContext db, ISchemaApi schemaApi)
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
            request.DisplayConfig
        );
        
        db.ContentTypes.Add(contentType);
        await db.SaveChangesAsync(cancellationToken);
        
        var schemaResult = await schemaApi.UpsertSchemaAsync(
            "ContentType", 
            contentType.Id.ToString(), 
            contentType.Name, 
            request.FieldsConfig, 
            cancellationToken);

        if (schemaResult.IsFailed)
        {
            return schemaResult.ToResult<ContentTypeDto>();
        }
        
        return Result.Ok(new ContentTypeDto
        {
            Id = contentType.Id,
            ProjectId = contentType.ProjectId,
            Key = contentType.Key,
            Name = contentType.Name,
            DisplayName = contentType.DisplayName,
            Description = contentType.Description,
            Icon = contentType.Icon,
            Color = contentType.Color,
            SortOrder = contentType.SortOrder,
            FieldsConfig = request.FieldsConfig,
            DisplayConfig = contentType.DisplayConfig
        });
    }
}
