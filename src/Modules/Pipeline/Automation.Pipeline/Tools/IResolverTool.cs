using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Tools;

public interface IResolverTool
{
    string Key { get; }
    string Label { get; }
    bool IsPure => false;
    IReadOnlyList<PinDefinition> Inputs { get; }
    IReadOnlyList<PinDefinition> Outputs { get; }

    Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    );
}
