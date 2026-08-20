using Automation.Pipeline.Domain.Entities;
using Automation.Pipeline.Engine.Models;
using Automation.Pipeline.Tools;

namespace Automation.Pipeline.Engine;

public interface IDagPlanner
{
    GraphValidationResult BuildAndValidateGraph(
        Domain.Entities.Pipeline pipeline,
        IReadOnlyList<NodeDefinition> customDefinitions,
        IToolRegistry toolRegistry,
        Dictionary<string, object?>? runtimeInputs = null
    );
}
