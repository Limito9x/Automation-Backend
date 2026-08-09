using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Automation.DynamicForms.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Features.ContentTypes.GetContentTypeById;

public class GetContentTypeByIdHandler(ContentDbContext db, ISchemaApi schemaApi)
{
    public async Task<Result<ContentTypeDto>> HandleAsync(
        GetContentTypeByIdQuery query,
        CancellationToken ct)
    {
        var item = await db.ContentTypes.FirstOrDefaultAsync(x => x.Id == query.Id, ct);
        if (item is null) return Result.Fail(new NotFoundError("ContentType not found"));
        
        var schemaResult = await schemaApi.GetActiveVersionAsync("ContentType", item.Id.ToString(), ct);

        return Result.Ok(new ContentTypeDto
        {
            Id = item.Id,
            ProjectId = item.ProjectId,
            Key = item.Key,
            Name = item.Name,
            DisplayName = item.DisplayName,
            Description = item.Description,
            Icon = item.Icon,
            Color = item.Color,
            SortOrder = item.SortOrder,
            FieldsConfig = schemaResult.IsSuccess ? schemaResult.Value.Fields : null,
            DisplayConfig = item.DisplayConfig
        });
    }
}
