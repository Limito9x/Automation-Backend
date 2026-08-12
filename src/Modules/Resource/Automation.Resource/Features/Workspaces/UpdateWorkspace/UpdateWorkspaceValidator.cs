namespace Automation.Resource.Features.Workspaces.UpdateWorkspace;

public class UpdateWorkspaceValidator : Validator<UpdateWorkspaceCommand>
{
    public UpdateWorkspaceValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RootPath).MaximumLength(500);
    }
}
