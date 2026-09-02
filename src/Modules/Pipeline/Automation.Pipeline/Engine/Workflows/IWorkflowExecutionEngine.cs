using Automation.Pipeline.Domain.Entities;

namespace Automation.Pipeline.Engine.Workflows;

public interface IWorkflowExecutionEngine
{
    Task<WorkflowExecution> ExecuteAsync(Workflow workflow, WorkflowEventContext context, CancellationToken ct = default);
}
