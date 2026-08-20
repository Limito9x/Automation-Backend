using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Tools.Utility;

/// <summary>
/// Tool nối 2 chuỗi dạng generic tương tự node Append (String) trong Unreal Engine (A + B).
/// </summary>
public class AppendStringTool : IResolverTool
{
    public string Key => "AppendString";
    public string Label => "Append String";
    public bool IsPure => true;

    public IReadOnlyList<PinDefinition> Inputs =>
        new List<PinDefinition>
        {
            new PinDefinition
            {
                Id = "A",
                Label = "A",
                PrimitiveType = PinPrimitiveType.String,
                Cardinality = PinCardinality.Single,
                IsRequired = false,
                DefaultValue = "",
            },
            new PinDefinition
            {
                Id = "B",
                Label = "B",
                PrimitiveType = PinPrimitiveType.String,
                Cardinality = PinCardinality.Single,
                IsRequired = false,
                DefaultValue = "",
            },
            new PinDefinition
            {
                Id = "Separator",
                Label = "Separator",
                PrimitiveType = PinPrimitiveType.String,
                Cardinality = PinCardinality.Single,
                IsRequired = false,
                DefaultValue = "",
            }
        };

    public IReadOnlyList<PinDefinition> Outputs =>
        new List<PinDefinition>
        {
            new PinDefinition
            {
                Id = "Result",
                Label = "Result",
                PrimitiveType = PinPrimitiveType.String,
                Cardinality = PinCardinality.Single,
                IsRequired = true,
            }
        };

    public Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    )
    {
        var a = inputs.TryGetValue("A", out var aObj) && aObj != null
            ? aObj.ToString() ?? string.Empty
            : string.Empty;

        var b = inputs.TryGetValue("B", out var bObj) && bObj != null
            ? bObj.ToString() ?? string.Empty
            : string.Empty;

        var separator = inputs.TryGetValue("Separator", out var sepObj) && sepObj != null
            ? sepObj.ToString() ?? string.Empty
            : string.Empty;

        var result = string.IsNullOrEmpty(separator) ? $"{a}{b}" : $"{a}{separator}{b}";

        return Task.FromResult(new Dictionary<string, object>
        {
            ["Result"] = result
        });
    }
}
