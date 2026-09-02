using System.Text.Json;
using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Features.Workflows.Dtos;
using Automation.Pipeline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Workflows.AddWorkflowNode;

public record AddWorkflowNodeCommand(
    Guid WorkflowId,
    string RefId,
    WorkflowNodeKind Kind,
    float PositionX,
    float PositionY,
    JsonDocument? Config = null
);

public class AddWorkflowNodeValidator : AbstractValidator<AddWorkflowNodeCommand>
{
    public AddWorkflowNodeValidator()
    {
        RuleFor(x => x.WorkflowId).NotEmpty();
        RuleFor(x => x.RefId).NotEmpty().MaximumLength(100);
    }
}

[Transactional(typeof(PipelineDbContext))]
public class AddWorkflowNodeHandler(PipelineDbContext db)
{
    public async Task<Result<WorkflowNodeDto>> HandleAsync(
        AddWorkflowNodeCommand command,
        CancellationToken ct
    )
    {
        var workflowExists = await db.Workflows.AsNoTracking().AnyAsync(x => x.Id == command.WorkflowId, ct);
        if (!workflowExists)
        {
            return Result.Fail<WorkflowNodeDto>($"Workflow with ID '{command.WorkflowId}' not found.");
        }

        var node = new WorkflowNode(
            Guid.NewGuid(),
            command.WorkflowId,
            command.RefId,
            command.Kind,
            command.PositionX,
            command.PositionY,
            command.Config
        );

        db.WorkflowNodes.Add(node);
        await db.SaveChangesAsync(ct);

        var dto = new WorkflowNodeDto(
            node.Id,
            node.WorkflowId,
            node.RefId,
            node.Kind,
            node.Position,
            node.Config
        );

        return Result.Ok(dto);
    }
}

public class AddWorkflowNodeEndpoint(IMessageBus bus) : Endpoint<AddWorkflowNodeCommand, WorkflowNodeDto>
{
    public override void Configure()
    {
        Post("{workflowId:guid}/nodes");
        Group<WorkflowsGroup>();
        Permissions(P.Workflow.Update);
        Description(d => d
            .Produces<WorkflowNodeDto>(200)
            .Produces(400)
            .Produces(404));
    }

    public override async Task HandleAsync(AddWorkflowNodeCommand req, CancellationToken ct)
    {
        var workflowId = Route<Guid>("workflowId");
        var cmd = req with { WorkflowId = workflowId };
        var result = await bus.InvokeAsync<Result<WorkflowNodeDto>>(cmd, ct);
        await this.SendResultAsync(result, ct);
    }
}
