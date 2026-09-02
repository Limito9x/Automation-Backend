namespace Automation.Pipeline.Features.Workflows;

public sealed class WorkflowsGroup : Group
{
    public WorkflowsGroup()
    {
        Configure("workflows", ep =>
        {
            ep.Description(x => x.WithTags("Workflows"));
        });
    }
}
