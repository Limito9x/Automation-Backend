using FluentValidation;

namespace Automation.Tag.Features.TagGroups.UpdateTagGroup;

public record UpdateTagGroupCommand(Guid Id, string Scope, string Name);

public class UpdateTagGroupValidator : Validator<UpdateTagGroupCommand>
{
    public UpdateTagGroupValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Scope).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}