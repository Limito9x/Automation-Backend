using Automation.Pipeline.Features.Workflows.Dtos;

namespace Automation.Pipeline.Features.Workflows.GetWorkflows;

public class GetWorkflowsEndpoint(IMessageBus bus) : Endpoint<GetWorkflowsQuery, List<WorkflowSummaryDto>>
{
    public override void Configure()
    {
        Get("");
        Group<WorkflowsGroup>();
        Permissions(P.Workflow.GetAll);
        Description(d => d
            .Produces<List<WorkflowSummaryDto>>(200)
            .Produces(400));
    }

    public override async Task HandleAsync(GetWorkflowsQuery req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<List<WorkflowSummaryDto>>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
