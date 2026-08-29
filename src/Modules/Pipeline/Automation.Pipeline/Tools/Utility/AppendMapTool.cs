using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Tools.Utility;

/// <summary>
/// Tool gộp 2 Map (Map Append) tương tự Unreal Engine.
/// Toàn bộ các cặp Key-Value từ Source Map sẽ được gộp vào Target Map (ghi đè nếu trùng Key).
/// </summary>
public class AppendMapTool : IResolverTool
{
    public string Key => "AppendMap";
    public string Label => "Append Map";
    public string? Category => "Utility";
    public bool IsPure => true;

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new()
        {
            Id = "TargetMap",
            Label = "Target Map",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Map,
            IsRequired = true
        },
        new()
        {
            Id = "SourceMap",
            Label = "Source Map",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Map,
            IsRequired = true
        }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new()
        {
            Id = "Result",
            Label = "Result Map",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Map,
            IsRequired = true
        }
    ];

    public Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    )
    {
        var map = new Dictionary<string, string>();

        void Merge(object? mapObj)
        {
            if (mapObj == null) return;
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
        }

        Merge(inputs.GetValueOrDefault("TargetMap") ?? inputs.GetValueOrDefault("targetmap"));
        Merge(inputs.GetValueOrDefault("SourceMap") ?? inputs.GetValueOrDefault("sourcemap"));

        return Task.FromResult(new Dictionary<string, object>
        {
            ["Result"] = map
        });
    }
}
