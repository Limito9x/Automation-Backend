namespace Automation.Notifications.Features.Notifications.Archive;

internal class ArchiveValidator : Validator<ArchiveCommand>
{
    public ArchiveValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");
    }
}

