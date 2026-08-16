namespace Automation.Inspection.Features.InspectorRules.CreateInspectorRule;

public class CreateInspectorRuleValidator : Validator<CreateInspectorRuleCommand>
{
    public CreateInspectorRuleValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.PlatformExtensionId).NotEmpty();
        RuleFor(x => x.InspectorId).NotEmpty();
    }
}
