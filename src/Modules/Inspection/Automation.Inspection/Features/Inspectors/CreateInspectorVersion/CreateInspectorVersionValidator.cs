namespace Automation.Inspection.Features.Inspectors.CreateInspectorVersion;

public class CreateInspectorVersionValidator : Validator<CreateInspectorVersionCommand>
{
    public CreateInspectorVersionValidator()
    {
        RuleFor(x => x.InspectorId).NotEmpty();
        RuleFor(x => x.EntryPoint).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ScriptHash).NotEmpty().MaximumLength(64);
        RuleFor(x => x.AssetId).NotEmpty();
    }
}
