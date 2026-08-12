namespace Automation.Resource.Features.Resources.UploadToRemoteWorkspace;

public class UploadToRemoteWorkspaceValidator : Validator<UploadToRemoteWorkspaceCommand>
{
    public UploadToRemoteWorkspaceValidator()
    {
        RuleFor(x => x.WorkspaceId).NotEmpty();
        RuleFor(x => x.Assets).NotEmpty();
        RuleForEach(x => x.Assets).ChildRules(a =>
        {
            a.RuleFor(x => x.AssetId).NotEmpty();
            a.RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        });
    }
}

