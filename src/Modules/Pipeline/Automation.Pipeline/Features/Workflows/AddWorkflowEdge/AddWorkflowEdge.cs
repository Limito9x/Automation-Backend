using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Features.Workflows.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Workflows.AddWorkflowEdge;

public record AddWorkflowEdgeCommand(
    Guid WorkflowId,
    Guid SourceWorkflowNodeId,
    string SourcePin,
    Guid TargetWorkflowNodeId,
    string TargetPin
);

public class AddWorkflowEdgeValidator : AbstractValidator<AddWorkflowEdgeCommand>
{
    public AddWorkflowEdgeValidator()
    {
        RuleFor(x => x.WorkflowId).NotEmpty();
        RuleFor(x => x.SourceWorkflowNodeId).NotEmpty();
        RuleFor(x => x.SourcePin).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TargetWorkflowNodeId).NotEmpty();
        RuleFor(x => x.TargetPin).NotEmpty().MaximumLength(100);
    }
}

[Transactional(typeof(PipelineDbContext))]
public class AddWorkflowEdgeHandler(PipelineDbContext db)
{
    public async Task<Result<WorkflowEdgeDto>> HandleAsync(
        AddWorkflowEdgeCommand command,
        CancellationToken ct
    )
    {
        var workflowExists = await db.Workflows.AsNoTracking().AnyAsync(x => x.Id == command.WorkflowId, ct);
        if (!workflowExists)
        {
            return Result.Fail<WorkflowEdgeDto>($"Workflow with ID '{command.WorkflowId}' not found.");
        }

        var sourceNodeExists = await db.WorkflowNodes.AsNoTracking().AnyAsync(x => x.Id == command.SourceWorkflowNodeId && x.WorkflowId == command.WorkflowId, ct);
        var targetNodeExists = await db.WorkflowNodes.AsNoTracking().AnyAsync(x => x.Id == command.TargetWorkflowNodeId && x.WorkflowId == command.WorkflowId, ct);

        if (!sourceNodeExists || !targetNodeExists)
        {
            return Result.Fail<WorkflowEdgeDto>("Source or Target node does not exist in this workflow.");
        }

        var edge = new WorkflowEdge(
            command.WorkflowId,
            command.SourceWorkflowNodeId,
            command.SourcePin,
            command.TargetWorkflowNodeId,
            command.TargetPin
        );

        db.WorkflowEdges.Add(edge);
        await db.SaveChangesAsync(ct);

        var dto = new WorkflowEdgeDto(
            edge.Id,
            edge.WorkflowId,
            edge.SourceWorkflowNodeId,
            edge.SourcePin,
            edge.TargetWorkflowNodeId,
            edge.TargetPin
        );

        return Result.Ok(dto);
    }
}

public class AddWorkflowEdgeEndpoint(IMessageBus bus) : Endpoint<AddWorkflowEdgeCommand, WorkflowEdgeDto>
{
    public override void Configure()
    {
        Post("{workflowId:guid}/edges");
        Group<WorkflowsGroup>();
        Permissions(P.Workflow.Update);
        Description(d => d
            .Produces<WorkflowEdgeDto>(200)
            .Produces(400)
            .Produces(404));
    }

    public override async Task HandleAsync(AddWorkflowEdgeCommand req, CancellationToken ct)
    {
        var workflowId = Route<Guid>("workflowId");
        var cmd = req with { WorkflowId = workflowId };
        var result = await bus.InvokeAsync<Result<WorkflowEdgeDto>>(cmd, ct);
        await this.SendResultAsync(result, ct);
    }
}
