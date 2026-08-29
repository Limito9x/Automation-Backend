using System.Collections;
using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Tools.Collections;

public class MergeMapsTool : IResolverTool
{
    public string Key => "MergeMaps";
    public string Label => "Merge Maps";
    public string? Category => "Collections";
    public string? Description => "Merges two Maps into one. Keys in MapB overwrite keys in MapA.";
    public bool IsPure => true;

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new() { Id = "MapA", Label = "Map A", PrimitiveType = PinPrimitiveType.String, Cardinality = PinCardinality.Map, IsRequired = true },
        new() { Id = "MapB", Label = "Map B", PrimitiveType = PinPrimitiveType.String, Cardinality = PinCardinality.Map, IsRequired = true }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new() { Id = "Map", Label = "Map", PrimitiveType = PinPrimitiveType.String, Cardinality = PinCardinality.Map }
    ];

    public Task<Dictionary<string, object>> ExecuteAsync(Dictionary<string, object> inputs, ToolExecutionContext context)
    {
        var merged = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        MergeInto(merged, inputs.GetValueOrDefault("MapA"));
        MergeInto(merged, inputs.GetValueOrDefault("MapB"));

        return Task.FromResult(new Dictionary<string, object>
        {
            ["Map"] = merged
        });
    }

    private static void MergeInto(Dictionary<string, object?> target, object? source)
    {
        if (source == null) return;

        if (source is IDictionary dict)
        {
            foreach (DictionaryEntry entry in dict)
            {
                if (entry.Key != null) target[entry.Key.ToString()!] = entry.Value;
            }
        }
        else if (source is JsonElement jsonElem && jsonElem.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in jsonElem.EnumerateObject())
            {
                target[prop.Name] = prop.Value.ValueKind switch
                {
                    JsonValueKind.String => prop.Value.GetString(),
                    JsonValueKind.Number => prop.Value.TryGetInt64(out var l) ? l : prop.Value.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => prop.Value.GetRawText()
                };
            }
        }
        else if (source is string jsonStr && jsonStr.TrimStart().StartsWith('{'))
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonStr);
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    target[prop.Name] = prop.Value.GetString() ?? prop.Value.GetRawText();
                }
            }
            catch { }
        }
    }
}
