using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Tools;

public interface IResolverTool
{
    string Key { get; }
    string Label { get; }
    IReadOnlyList<string> Aliases => [];
    bool IsPure => false;
    string? Category => null;
    IReadOnlyList<PinDefinition> Inputs { get; }
    IReadOnlyList<PinDefinition> Outputs { get; }

    (IReadOnlyList<PinDefinition> Inputs, IReadOnlyList<PinDefinition> Outputs) ResolvePins(
        Dictionary<string, object?>? configValues,
        IPinResolutionContext? context = null
    ) => (Inputs, Outputs);

    Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    );
}
