using Automation.Pipeline.Features.Workflows.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Workflows.GetWorkflowExecution;

public record GetWorkflowExecutionQuery(Guid ExecutionId);

[NonTransactional]
public class GetWorkflowExecutionHandler(PipelineDbContext db)
{
    public async Task<Result<WorkflowExecutionDto>> HandleAsync(
        GetWorkflowExecutionQuery query,
        CancellationToken ct
    )
    {
        var execution = await db.WorkflowExecutions
            .AsNoTracking()
            .Include(x => x.NodeExecutions)
            .FirstOrDefaultAsync(x => x.Id == query.ExecutionId, ct);

        if (execution == null)
        {
            return Result.Fail<WorkflowExecutionDto>($"WorkflowExecution with ID '{query.ExecutionId}' not found.");
        }

        var dto = new WorkflowExecutionDto(
            execution.Id,
            execution.WorkflowId,
            execution.TriggerEventType,
            execution.TriggerPayload,
            execution.Status,
            execution.StartedAt,
            execution.FinishedAt,
            execution.ErrorMessage,
            execution.NodeExecutions.Select(ne => new WorkflowNodeExecutionDto(
                ne.Id,
                ne.WorkflowExecutionId,
                ne.WorkflowNodeId,
                ne.Status,
                ne.StartedAt,
                ne.FinishedAt,
                ne.Output,
                ne.ErrorMessage
            )).ToList()
        );

        return Result.Ok(dto);
    }
}

public class GetWorkflowExecutionEndpoint(IMessageBus bus) : EndpointWithoutRequest<WorkflowExecutionDto>
{
    public override void Configure()
    {
        Get("executions/{executionId:guid}");
        Group<WorkflowsGroup>();
        Permissions(P.Workflow.GetById);
        Description(d => d
            .Produces<WorkflowExecutionDto>(200)
            .Produces(404));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var executionId = Route<Guid>("executionId");
        var result = await bus.InvokeAsync<Result<WorkflowExecutionDto>>(new GetWorkflowExecutionQuery(executionId), ct);
        await this.SendResultAsync(result, ct);
    }
}
