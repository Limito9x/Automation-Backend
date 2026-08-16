namespace Automation.Inspection.Features.Inspectors.CreateInspector;

public class CreateInspectorValidator : Validator<CreateInspectorCommand>
{
    public CreateInspectorValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Key).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ExecutorKey).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
