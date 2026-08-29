using System.Collections;
using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Tools.Collections;

public class ZipToMapTool : IResolverTool
{
    public string Key => "ZipToMap";
    public string Label => "Zip To Map";
    public string? Category => "Collections";
    public string? Description => "Zips an Array of Keys and an Array of Values into a single Map.";
    public bool IsPure => true;

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new() { Id = "Keys", Label = "Keys", PrimitiveType = PinPrimitiveType.String, Cardinality = PinCardinality.Array, IsRequired = true },
        new() { Id = "Values", Label = "Values", PrimitiveType = PinPrimitiveType.String, Cardinality = PinCardinality.Array, IsRequired = true }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new() { Id = "Map", Label = "Map", PrimitiveType = PinPrimitiveType.String, Cardinality = PinCardinality.Map }
    ];

    public Task<Dictionary<string, object>> ExecuteAsync(Dictionary<string, object> inputs, ToolExecutionContext context)
    {
        var keys = ExtractStringList(inputs.GetValueOrDefault("Keys"));
        var values = ExtractObjectList(inputs.GetValueOrDefault("Values"));

        var map = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        var limit = Math.Min(keys.Count, values.Count);
        for (var i = 0; i < limit; i++)
        {
            map[keys[i]] = values[i];
        }

        return Task.FromResult(new Dictionary<string, object>
        {
            ["Map"] = map
        });
    }

    private static List<string> ExtractStringList(object? raw)
    {
        var list = new List<string>();
        if (raw == null) return list;

        if (raw is IEnumerable<string> strEnum) return strEnum.ToList();

        if (raw is IEnumerable enumerable && raw is not string)
        {
            foreach (var item in enumerable)
            {
                if (item != null) list.Add(item.ToString()!);
            }
            return list;
        }

        if (raw is JsonElement jsonElem && jsonElem.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in jsonElem.EnumerateArray())
            {
                list.Add(item.GetString() ?? item.GetRawText());
            }
            return list;
        }

        list.Add(raw.ToString()!);
        return list;
    }

    private static List<object?> ExtractObjectList(object? raw)
    {
        var list = new List<object?>();
        if (raw == null) return list;

        if (raw is IEnumerable enumerable && raw is not string)
        {
            foreach (var item in enumerable) list.Add(item);
            return list;
        }

        if (raw is JsonElement jsonElem && jsonElem.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in jsonElem.EnumerateArray())
            {
                list.Add(item.ValueKind switch
                {
                    JsonValueKind.String => item.GetString(),
                    JsonValueKind.Number => item.TryGetInt64(out var l) ? l : item.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => item.GetRawText()
                });
            }
            return list;
        }

        list.Add(raw);
        return list;
    }
}
