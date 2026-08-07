namespace Automation.Projects.Features.Projects.CreateProject;

internal class CreateProjectValidator : Validator<CreateProjectCommand>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);
    }
}
