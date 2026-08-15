namespace Automation.Workspace.Features.Resources;

public class ResourcesGroup : Group
{
    public ResourcesGroup()
    {
        Configure(
            "",
            ep =>
            {
                ep.Description(x =>
                    x.Produces(StatusCodes.Status401Unauthorized)
                        .Produces(StatusCodes.Status403Forbidden)
                        .WithTags("Resources")
                );
            }
        );
    }
}
