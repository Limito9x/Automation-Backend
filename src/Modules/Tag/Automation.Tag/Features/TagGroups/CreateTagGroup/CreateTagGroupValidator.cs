namespace Automation.Tag.Features.TagGroups.CreateTagGroup;

public class CreateTagGroupValidator : Validator<CreateTagGroupCommand>
{
    public CreateTagGroupValidator()
    {
        RuleFor(x => x.Scope).NotEmpty().WithMessage("Scope is required").MaximumLength(100);

        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required").MaximumLength(100);
    }
}
