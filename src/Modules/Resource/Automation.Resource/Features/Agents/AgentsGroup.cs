namespace Automation.Resource.Features.Agents;

public class AgentsGroup : Group
{
    public AgentsGroup()
    {
        Configure("agents", ep =>
        {
            ep.Description(x => x
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .WithTags("Agents"));
        });
    }
}
