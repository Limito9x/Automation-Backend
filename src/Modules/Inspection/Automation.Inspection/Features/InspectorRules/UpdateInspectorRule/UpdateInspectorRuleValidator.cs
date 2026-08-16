namespace Automation.Inspection.Features.InspectorRules.UpdateInspectorRule;

public class UpdateInspectorRuleValidator : Validator<UpdateInspectorRuleCommand>
{
    public UpdateInspectorRuleValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PlatformExtensionId).NotEmpty();
    }
}
