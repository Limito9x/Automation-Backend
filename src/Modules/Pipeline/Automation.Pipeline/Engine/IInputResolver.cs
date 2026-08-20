using Automation.Pipeline.Engine.Models;

namespace Automation.Pipeline.Engine;

public interface IInputResolver
{
    Dictionary<string, object> ResolveInputs(DagNode node, PipelineExecutionState state);
}
