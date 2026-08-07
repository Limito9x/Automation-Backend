namespace Automation.Content.Features.ContentItems.CreateContentItem;

internal class CreateContentItemValidator : Validator<CreateContentItemCommand>
{
    public CreateContentItemValidator()
    {
        RuleFor(x => x.ContentTypeId).NotEmpty();
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Values).NotNull();
    }
}
