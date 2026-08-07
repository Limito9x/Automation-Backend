using Automation.Content.Domain.Entities;
using Automation.Content.Infrastructure.Persistence;
using Automation.Content.Shared.Dtos;
using Gridify;
using Microsoft.EntityFrameworkCore;

namespace Automation.Content.Features.ContentTypes.GetContentTypes;

public class GetContentTypesHandler(ContentDbContext db)
{
    public async Task<Result<PagedResult<ContentTypeDto>>> HandleAsync(
        GetContentTypesQuery query,
        CancellationToken ct)
    {
        var mapper = new GridifyMapper<ContentType>()
            .GenerateMappings();

        var queryable = db.Set<ContentType>().AsQueryable();
        
        if (query.ProjectId != Guid.Empty)
        {
            queryable = queryable.Where(x => x.ProjectId == query.ProjectId);
        }

        var result = await queryable
            .ToPagedResultAsync<ContentType, ContentTypeDto>(query, mapper, ct);
            
        return result;
    }
}
