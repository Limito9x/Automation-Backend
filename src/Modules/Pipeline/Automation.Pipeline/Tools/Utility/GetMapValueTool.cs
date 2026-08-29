using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Tools.Utility;

/// <summary>
/// Tool tra cứu giá trị theo Key trong Map (Find / Get Map Value) tương tự Unreal Engine.
/// </summary>
public class GetMapValueTool : IResolverTool
{
    public string Key => "GetMapValue";
    public string Label => "Get Map Value";
    public string? Category => "Utility";
    public bool IsPure => true;

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new()
        {
            Id = "Map",
            Label = "Map",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Map,
            IsRequired = true
        },
        new()
        {
            Id = "Key",
            Label = "Key",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single,
            IsRequired = true
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
        },
        new()
        {
            Id = "Found",
            Label = "Found",
            PrimitiveType = PinPrimitiveType.Boolean,
            Cardinality = PinCardinality.Single,
            IsRequired = true
        }
    ];

    public Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    )
    {
        var mapObj = inputs.GetValueOrDefault("Map") ?? inputs.GetValueOrDefault("map");
        var key = inputs.GetValueOrDefault("Key")?.ToString() ?? string.Empty;

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (mapObj is Dictionary<string, string> dStr)
        {
            foreach (var (k, v) in dStr) map[k] = v;
        }
        else if (mapObj is Dictionary<string, object?> dObj)
        {
            foreach (var (k, v) in dObj) map[k] = v?.ToString() ?? string.Empty;
        }
        else if (mapObj is string jsonStr && jsonStr.TrimStart().StartsWith('{'))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonStr);
                if (parsed != null)
                {
                    foreach (var (k, v) in parsed) map[k] = v;
                }
            }
            catch { }
        }

        var found = map.TryGetValue(key, out var val);

        return Task.FromResult(new Dictionary<string, object>
        {
            ["Value"] = val ?? string.Empty,
            ["Found"] = found
        });
    }
}
