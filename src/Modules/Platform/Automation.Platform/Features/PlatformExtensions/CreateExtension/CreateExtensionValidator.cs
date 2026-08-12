namespace Automation.Platform.Features.PlatformExtensions.CreateExtension;

public class CreateExtensionValidator : Validator<CreateExtensionCommand>
{
    public CreateExtensionValidator()
    {
        RuleFor(x => x.Extension)
            .NotEmpty()
            .MaximumLength(50)
            .Must(x => x.StartsWith('.')).WithMessage("Extension must start with a dot (e.g. '.blend', '.fbx').");
    }
}
