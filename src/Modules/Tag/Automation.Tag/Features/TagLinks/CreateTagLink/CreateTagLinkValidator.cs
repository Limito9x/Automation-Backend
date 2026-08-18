namespace Automation.Tag.Features.TagLinks.CreateTagLink;

public class CreateTagLinkValidator : Validator<CreateTagLinkCommand>
{
    public CreateTagLinkValidator()
    {
        RuleFor(x => x.TagId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
    }
}
