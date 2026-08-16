namespace Automation.Inspection.Features.Inspections.TriggerInspection;

public class TriggerInspectionValidator : Validator<TriggerInspectionCommand>
{
    public TriggerInspectionValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.ResourceVersionIds).NotEmpty();
    }
}
