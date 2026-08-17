namespace Automation.Inspection.Features.Inspectors.UpdateInspector;

public class UpdateInspectorValidator : Validator<UpdateInspectorCommand>
{
    public UpdateInspectorValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
