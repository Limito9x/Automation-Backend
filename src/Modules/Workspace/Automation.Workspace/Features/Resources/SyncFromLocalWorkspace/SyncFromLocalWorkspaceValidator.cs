namespace Automation.Workspace.Features.Resources.SyncFromLocalWorkspace;

public class SyncFromLocalWorkspaceValidator : Validator<SyncFromLocalWorkspaceCommand>
{
    public SyncFromLocalWorkspaceValidator()
    {
        RuleFor(x => x.WorkspaceId).NotEmpty();
        RuleFor(x => x.Files).NotNull();
        RuleForEach(x => x.Files).ChildRules(f =>
        {
            f.RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
            f.RuleFor(x => x.FilePath).NotEmpty().MaximumLength(500);
        });
    }
}

