using Automation.Resource.Domain.Enums;

namespace Automation.Resource.Features.Workspaces.CreateWorkspace;

public class CreateWorkspaceValidator : Validator<CreateWorkspaceCommand>
{
    public CreateWorkspaceValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.PlatformId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Kind).IsInEnum();

        When(x => x.Kind == WorkspaceKind.Local, () =>
        {
            RuleFor(x => x.AgentId)
                .NotEmpty()
                .WithMessage("AgentId is required when WorkspaceKind is Local.");
        });
    }
}
