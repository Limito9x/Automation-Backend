namespace Automation.Inspection.Features.InspectorRules;

public class InspectorRulesGroup : Group
{
    public InspectorRulesGroup()
    {
        Configure("inspector-rules", ep =>
        {
            ep.Description(x => x.WithTags("InspectorRules"));
        });
    }
}
