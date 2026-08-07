namespace Automation.Projects.Features.Projects.UpdateProject;

internal class UpdateProjectValidator : Validator<UpdateProjectCommand>
{
    public UpdateProjectValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
            
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);
    }
}
