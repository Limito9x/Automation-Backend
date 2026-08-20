using Automation.Pipeline.Engine.Messages;
using Automation.Pipeline.Engine.Models;

namespace Automation.Pipeline.Engine;

public interface IAgentBatchBuilder
{
    StageTaskMessage BuildBatchTask(
        Guid pipelineExecutionId,
        IReadOnlyList<DagNode> batchNodes,
        PipelineExecutionState state,
        IInputResolver inputResolver
    );
}
