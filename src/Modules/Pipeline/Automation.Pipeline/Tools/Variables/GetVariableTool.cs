using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Engine.DataResolver;

namespace Automation.Pipeline.Tools.Variables;

public class GetVariableTool(IExecutionMemoryStore? memoryStore = null) : IResolverTool
{
    public string Key => "GetVariable";
    public string Label => "Get Variable";
    public string? Category => "Variables";
    public bool IsPure => true;

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new()
        {
            Id = "VariableName",
            Label = "Variable Name",
            PrimitiveType = PinPrimitiveType.Variable,
            Cardinality = PinCardinality.Single,
            IsRequired = true,
            DefaultValue = ""
        }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new()
        {
            Id = "Value",
            Label = "Value",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single,
            IsRequired = true
        }
    ];

    public async Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    )
    {
        var varName = inputs.GetValueOrDefault("VariableName")?.ToString()
                      ?? inputs.GetValueOrDefault("variablename")?.ToString()
                      ?? "MyVar";

        object? val = null;
        if (context.PipelineExecutionId != Guid.Empty && !string.IsNullOrWhiteSpace(varName) && memoryStore != null)
        {
            val = await memoryStore.GetVariableAsync(context.PipelineExecutionId, varName, context.CancellationToken);
        }

        val ??= inputs.GetValueOrDefault("Value") ?? inputs.GetValueOrDefault("value") ?? string.Empty;

        return new Dictionary<string, object>
        {
            ["Value"] = val
        };
    }
}
