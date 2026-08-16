namespace Automation.Inspection.Features.Inspections;

public class InspectionsGroup : Group
{
    public InspectionsGroup()
    {
        Configure("inspections", ep =>
        {
            ep.Description(x => x.WithTags("Inspections"));
        });
    }
}
