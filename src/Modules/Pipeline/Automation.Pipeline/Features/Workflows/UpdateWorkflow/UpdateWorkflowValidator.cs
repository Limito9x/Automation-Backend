namespace Automation.Pipeline.Features.Workflows.UpdateWorkflow;

public class UpdateWorkflowValidator : AbstractValidator<UpdateWorkflowCommand>
{
    public UpdateWorkflowValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
