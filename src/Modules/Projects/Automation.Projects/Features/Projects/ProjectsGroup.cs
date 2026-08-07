using FastEndpoints;

namespace Automation.Projects.Features.Projects;

internal sealed class ProjectsGroup : Group
{
    public ProjectsGroup()
    {
        Configure("/projects", ep =>
        {
            ep.Description(b => b.WithTags("Projects"));
        });
    }
}
