namespace Automation.Workspace.Features.Workspaces;

public class WorkspacesGroup : Group
{
    public WorkspacesGroup()
    {
        Configure("workspaces", ep =>
        {
            ep.Description(x => x
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .WithTags("Workspaces"));
        });
    }
}

