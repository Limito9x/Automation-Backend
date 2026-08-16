namespace Automation.Inspection.Features.Inspectors;

public class InspectorsGroup : Group
{
    public InspectorsGroup()
    {
        Configure("inspectors", ep =>
        {
            ep.Description(x => x.WithTags("Inspectors"));
        });
    }
}
