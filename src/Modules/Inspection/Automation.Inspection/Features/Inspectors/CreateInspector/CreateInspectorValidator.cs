namespace Automation.Inspection.Features.Inspectors.CreateInspector;

public class CreateInspectorValidator : Validator<CreateInspectorCommand>
{
    public CreateInspectorValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ExecutorKey).NotEmpty().MaximumLength(50);
        RuleFor(x => x.EntryPoint).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ScriptHash).NotEmpty();
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
