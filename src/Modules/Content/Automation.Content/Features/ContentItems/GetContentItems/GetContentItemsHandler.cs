using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Gridify;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Features.ContentItems.GetContentItems;

public class GetContentItemsHandler(ContentDbContext db)
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
            
        return result;
    }
}
