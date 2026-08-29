using System.Collections;
using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Tools.Collections;

public class GetMapValuesTool : IResolverTool
{
    public string Key => "GetMapValues";
    public string Label => "Get Map Values";
    public string? Category => "Collections";
    public string? Description => "Extracts all values from a Map into an Array.";
    public bool IsPure => true;

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new() { Id = "Map", Label = "Map", PrimitiveType = PinPrimitiveType.String, Cardinality = PinCardinality.Map, IsRequired = true }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new() { Id = "Values", Label = "Values", PrimitiveType = PinPrimitiveType.String, Cardinality = PinCardinality.Array }
    ];

    public Task<Dictionary<string, object>> ExecuteAsync(Dictionary<string, object> inputs, ToolExecutionContext context)
    {
        var valuesList = new List<object>();

        if (inputs.TryGetValue("Map", out var rawMap) && rawMap != null)
        {
            if (rawMap is IDictionary dict)
            {
                foreach (var v in dict.Values)
                {
                    if (v != null) valuesList.Add(v);
                }
            }
            else if (rawMap is JsonElement jsonElem && jsonElem.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in jsonElem.EnumerateObject())
                {
                    valuesList.Add(prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString() ?? "",
                        JsonValueKind.Number => prop.Value.TryGetInt64(out var l) ? l : prop.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => prop.Value.GetRawText()
                    });
                }
            }
            else if (rawMap is string jsonStr && jsonStr.TrimStart().StartsWith('{'))
            {
                try
                {
                    using var doc = JsonDocument.Parse(jsonStr);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        valuesList.Add(prop.Value.GetString() ?? prop.Value.GetRawText());
                    }
                }
                catch { }
            }
        }

        return Task.FromResult(new Dictionary<string, object>
        {
            ["Values"] = valuesList
        });
    }
}
