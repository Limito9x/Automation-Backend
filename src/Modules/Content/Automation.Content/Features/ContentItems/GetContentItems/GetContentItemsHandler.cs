using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Automation.DynamicForms.Contracts;
using Gridify;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Features.ContentItems.GetContentItems;

public class GetContentItemsHandler(ContentDbContext db, ISchemaApi schemaApi)
{
    public async Task<Result<PagedResult<ContentItemDto>>> HandleAsync(
        GetContentItemsQuery query,
        CancellationToken ct)
    {
        var mapper = new GridifyMapper<ContentItem>()
            .GenerateMappings();

        var queryable = db.Set<ContentItem>().AsQueryable();
        
        if (query.ProjectId.HasValue && query.ProjectId.Value != Guid.Empty)
        {
            queryable = queryable.Where(x => x.ProjectId == query.ProjectId.Value);
        }
        
        if (query.ContentTypeId.HasValue && query.ContentTypeId.Value != Guid.Empty)
        {
            queryable = queryable.Where(x => x.ContentTypeId == query.ContentTypeId.Value);
        }

        var result = await queryable
            .ToPagedResultAsync<ContentItem, ContentItemDto>(query, mapper, ct);

        if (result.IsSuccess)
        {
            // Fetch schema data for each content item (MVP N+1 approach)
            foreach (var item in result.Value.Items)
            {
                var dataResult = await schemaApi.GetDataAsync(item.Id.ToString(), "ContentItem", ct);
                if (dataResult.IsSuccess)
                {
                    item.Values = dataResult.Value.Values;
                }
            }
        }
            
        return result;
    }
}
