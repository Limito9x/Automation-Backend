using Automation.Pipeline.Features.Workflows.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Workflows.GetWorkflowExecutions;

public record GetWorkflowExecutionsQuery(Guid WorkflowId);

[NonTransactional]
public class GetWorkflowExecutionsHandler(PipelineDbContext db)
{
    public async Task<Result<List<WorkflowExecutionDto>>> HandleAsync(
        GetWorkflowExecutionsQuery query,
        CancellationToken ct
    )
    {
        var executions = await db.WorkflowExecutions
            .AsNoTracking()
            .Where(x => x.WorkflowId == query.WorkflowId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(50)
            .Select(x => new WorkflowExecutionDto(
                x.Id,
                x.WorkflowId,
                x.TriggerEventType,
                x.TriggerPayload,
                x.Status,
                x.StartedAt,
                x.FinishedAt,
                x.ErrorMessage,
                null
            ))
            .ToListAsync(ct);

        return Result.Ok(executions);
    }
}

public class GetWorkflowExecutionsEndpoint(IMessageBus bus) : EndpointWithoutRequest<List<WorkflowExecutionDto>>
{
    public override void Configure()
    {
        Get("{id:guid}/executions");
        Group<WorkflowsGroup>();
        Permissions(P.Workflow.GetAll);
        Description(d => d
            .Produces<List<WorkflowExecutionDto>>(200)
            .Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await bus.InvokeAsync<Result<List<WorkflowExecutionDto>>>(new GetWorkflowExecutionsQuery(id), ct);
        await this.SendResultAsync(result, ct);
    }
}
