using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Automation.DynamicForms.Contracts;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;
using Automation.SharedKernel.Errors;

namespace Automation.Content.Features.ContentItems.GetContentItems;

[NonTransactional]
public class GetContentItemsHandler(ContentDbContext db, ISchemaApi schemaApi)
{
    public async Task<Result<PagedResult<ContentItemDto>>> HandleAsync(
        GetContentItemsQuery query,
        CancellationToken ct)
    {
        var mapper = new GridifyMapper<ContentItem>()
            .GenerateMappings();

        var queryable = db.ContentItems.AsQueryable();

        queryable = queryable.Where(x => x.ProjectId == query.ProjectId);

        if (!string.IsNullOrEmpty(query.Key))
        {
            if(Guid.TryParse(query.Key, out var contentTypeId))
            {
                queryable = queryable.Where(x => x.ContentTypeId == contentTypeId);
            }
            else
            {
                var contentType = await db.ContentTypes.FirstOrDefaultAsync(c => c.Key == query.Key, ct);
                if (contentType == null)
                {
                    return Result.Fail(new NotFoundError($"ContentType with key {query.Key} not found"));
                }
                queryable = queryable.Where(x => x.ContentTypeId == contentType.Id);
            }
        }

        var result = await queryable
            .ToPagedResultAsync<ContentItem, ContentItemDto>(query, mapper, ct);

        if (result.IsSuccess && result.Value.Items.Any())
        {
            // Fetch schema data for all content items in one query (Bulk approach)
            var itemIds = result.Value.Items.Select(i => i.Id.ToString()).ToList();
            var dataResult = await schemaApi.GetMultipleDataAsync(itemIds, "ContentItem", ct);
            
            if (dataResult.IsSuccess)
            {
                var valuesMap = dataResult.Value.ToDictionary(d => d.ClientId, d => d.Values);
                foreach (var item in result.Value.Items)
                {
                    if (valuesMap.TryGetValue(item.Id.ToString(), out var values))
                    {
                        item.Values = values;
                    }
                }
            }
        }
            
        return result;
    }
}
