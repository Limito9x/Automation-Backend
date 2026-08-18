using FluentValidation;

namespace Automation.Tag.Features.Tags.UpdateTag;

public record UpdateTagCommand(Guid Id, string Name, string? Color = null);

public class UpdateTagValidator : Validator<UpdateTagCommand>
{
    public UpdateTagValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Color).MaximumLength(50);
    }
}