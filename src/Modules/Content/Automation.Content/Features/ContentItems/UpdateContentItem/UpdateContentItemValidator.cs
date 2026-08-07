namespace Automation.Content.Features.ContentItems.UpdateContentItem;

internal class UpdateContentItemValidator : Validator<UpdateContentItemCommand>
{
    public UpdateContentItemValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Values).NotNull();
    }
}
