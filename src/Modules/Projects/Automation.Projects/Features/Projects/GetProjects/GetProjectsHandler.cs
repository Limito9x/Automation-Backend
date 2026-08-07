using Automation.Projects.Domain.Entities;
using Automation.Projects.Infrastructure.Persistence;
using Automation.Projects.Shared.Dtos;
using Gridify;

namespace Automation.Projects.Features.Projects.GetProjects;

public class GetProjectsHandler(ProjectsDbContext db)
{
    public async Task<Result<PagedResult<ProjectDto>>> HandleAsync(
        GetProjectsQuery query,
        CancellationToken ct)
    {
        var mapper = new GridifyMapper<Project>()
            .GenerateMappings();

        var result = await db.Set<Project>()
            .ToPagedResultAsync<Project, ProjectDto>(query, mapper, ct);
            
        return result;
    }
}
