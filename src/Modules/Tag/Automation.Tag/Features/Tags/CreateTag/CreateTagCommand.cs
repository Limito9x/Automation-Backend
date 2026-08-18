using FluentValidation;

namespace Automation.Tag.Features.Tags.CreateTag;

public record CreateTagCommand(Guid TagGroupId, string Name, string? Color = null);

public class CreateTagValidator : Validator<CreateTagCommand>
{
    public CreateTagValidator()
    {
        RuleFor(x => x.TagGroupId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Color).MaximumLength(50);
    }
}