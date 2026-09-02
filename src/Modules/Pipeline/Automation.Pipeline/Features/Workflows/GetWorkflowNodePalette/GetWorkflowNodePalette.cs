using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Features.Workflows.Dtos;
using Wolverine.Attributes;

namespace Automation.Pipeline.Features.Workflows.GetWorkflowNodePalette;

public record GetWorkflowNodePaletteQuery;

[NonTransactional]
public class GetWorkflowNodePaletteHandler
{
    public Task<Result<List<WorkflowNodePaletteItemDto>>> HandleAsync(
        GetWorkflowNodePaletteQuery query,
        CancellationToken ct
    )
    {
        var items = new List<WorkflowNodePaletteItemDto>
        {
            new(
                Kind: nameof(WorkflowNodeKind.EventTrigger),
                Name: "On Resource Created",
                Category: "Events",
                Description: "Triggered automatically when a new resource version is created/uploaded in a workspace.",
                InputPins: new List<string>(),
                OutputPins: new List<string> { "exec_out" }
            ),
            new(
                Kind: nameof(WorkflowNodeKind.ConditionFilter),
                Name: "Condition Filter",
                Category: "Logic",
                Description: "Filters execution by file extension, workspace, or path pattern, splitting the execution flow into True and False branches.",
                InputPins: new List<string> { "exec_in" },
                OutputPins: new List<string> { "true_out", "false_out" }
            ),
            new(
                Kind: nameof(WorkflowNodeKind.ExecutePipeline),
                Name: "Execute Pipeline",
                Category: "Pipelines",
                Description: "Invokes an existing Pipeline with auto-mapped runtime inputs from the triggering event.",
                InputPins: new List<string> { "exec_in" },
                OutputPins: new List<string> { "exec_out" }
            ),
            new(
                Kind: nameof(WorkflowNodeKind.SendNotification),
                Name: "Send Webhook / Notify",
                Category: "Integrations",
                Description: "Sends an HTTP POST webhook notification to external services (Discord, Slack, Webhook receiver).",
                InputPins: new List<string> { "exec_in" },
                OutputPins: new List<string> { "exec_out" }
            )
        };

        return Task.FromResult(Result.Ok(items));
    }
}

public class GetWorkflowNodePaletteEndpoint(IMessageBus bus) : EndpointWithoutRequest<List<WorkflowNodePaletteItemDto>>
{
    public override void Configure()
    {
        Get("palette");
        Group<WorkflowsGroup>();
        Permissions(P.Workflow.GetAll);
        Description(d => d
            .Produces<List<WorkflowNodePaletteItemDto>>(200));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<List<WorkflowNodePaletteItemDto>>>(new GetWorkflowNodePaletteQuery(), ct);
        await this.SendResultAsync(result, ct);
    }
}
