namespace Automation.Notifications.Features.Notifications.MarkAsRead;

internal class MarkAsReadValidator : Validator<MarkAsReadCommand>
{
    public MarkAsReadValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage("Ids is required");
    }
}


