using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Engine.Workflows;
using Automation.Pipeline.Features.Workflows.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Workflows.TriggerWorkflow;

public record TriggerWorkflowCommand(
    Guid Id,
    Guid WorkspaceId,
    Guid AgentId,
    List<Guid>? ResourceVersionIds = null,
    string? RelativePath = null,
    string? Extension = null
);

public class TriggerWorkflowValidator : AbstractValidator<TriggerWorkflowCommand>
{
    public TriggerWorkflowValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.WorkspaceId).NotEmpty();
        RuleFor(x => x.AgentId).NotEmpty();
    }
}

[NonTransactional]
public class TriggerWorkflowHandler(
    PipelineDbContext db,
    IWorkflowExecutionEngine engine
)
{
    public async Task<Result<WorkflowExecutionDto>> HandleAsync(
        TriggerWorkflowCommand command,
        CancellationToken ct
    )
    {
        var workflow = await db.Workflows
            .Include(w => w.Nodes)
            .Include(w => w.Edges)
            .FirstOrDefaultAsync(w => w.Id == command.Id, ct);

        if (workflow == null)
        {
            return Result.Fail<WorkflowExecutionDto>($"Workflow with ID '{command.Id}' not found.");
        }

        var context = new WorkflowEventContext
        {
            EventType = WorkflowEventType.OnResourceCreated,
            ProjectId = workflow.ProjectId,
            WorkspaceId = command.WorkspaceId,
            AgentId = command.AgentId,
            ResourceVersionIds = command.ResourceVersionIds ?? new List<Guid>(),
            RelativePath = command.RelativePath,
            Extension = command.Extension
        };

        var execution = await engine.ExecuteAsync(workflow, context, ct);

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

public class TriggerWorkflowEndpoint(IMessageBus bus) : Endpoint<TriggerWorkflowCommand, WorkflowExecutionDto>
{
    public override void Configure()
    {
        Post("{id:guid}/trigger");
        Group<WorkflowsGroup>();
        Permissions(P.Workflow.Update);
        Description(d => d
            .Produces<WorkflowExecutionDto>(200)
            .Produces(400)
            .Produces(404));
    }

    public override async Task HandleAsync(TriggerWorkflowCommand req, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var cmd = req with { Id = id };
        var result = await bus.InvokeAsync<Result<WorkflowExecutionDto>>(cmd, ct);
        await this.SendResultAsync(result, ct);
    }
}
