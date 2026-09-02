using Automation.Pipeline.Features.Workflows.Dtos;

namespace Automation.Pipeline.Features.Workflows.CreateWorkflow;

public class CreateWorkflowEndpoint(IMessageBus bus) : Endpoint<CreateWorkflowCommand, WorkflowSummaryDto>
{
    public override void Configure()
    {
        Post("");
        Group<WorkflowsGroup>();
        Permissions(P.Workflow.Create);
        Description(d => d
            .Produces<WorkflowSummaryDto>(200)
            .Produces(400));
    }

    public override async Task HandleAsync(CreateWorkflowCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<WorkflowSummaryDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
