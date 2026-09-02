using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.Pipeline.Constants;

public class PipelinePermissions
{
    public static PipelineFeature Pipeline { get; } = new();
    public static WorkflowFeature Workflow { get; } = new();

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() => new()
    {
        { "Pipeline", Pipeline.All },
        { "Workflow", Workflow.All }
    };

    public class PipelineFeature() : BaseCrudPermission("pipeline") { }
    public class WorkflowFeature() : BaseCrudPermission("workflow") { }
}
