using System.Collections;
using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Tools.Collections;

public class GetMapKeysTool : IResolverTool
{
    public string Key => "GetMapKeys";
    public string Label => "Get Map Keys";
    public string? Category => "Collections";
    public string? Description => "Extracts all keys from a Map into a string Array.";
    public bool IsPure => true;

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new() { Id = "Map", Label = "Map", PrimitiveType = PinPrimitiveType.String, Cardinality = PinCardinality.Map, IsRequired = true }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new() { Id = "Keys", Label = "Keys", PrimitiveType = PinPrimitiveType.String, Cardinality = PinCardinality.Array }
    ];

    public Task<Dictionary<string, object>> ExecuteAsync(Dictionary<string, object> inputs, ToolExecutionContext context)
    {
        var keysList = new List<string>();

        if (inputs.TryGetValue("Map", out var rawMap) && rawMap != null)
        {
            if (rawMap is IDictionary dict)
            {
                foreach (var k in dict.Keys)
                {
                    if (k != null) keysList.Add(k.ToString()!);
                }
            }
            else if (rawMap is JsonElement jsonElem && jsonElem.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in jsonElem.EnumerateObject())
                {
                    keysList.Add(prop.Name);
                }
            }
            else if (rawMap is string jsonStr && jsonStr.TrimStart().StartsWith('{'))
            {
                try
                {
                    using var doc = JsonDocument.Parse(jsonStr);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        keysList.Add(prop.Name);
                    }
                }
                catch { }
            }
        }

        return Task.FromResult(new Dictionary<string, object>
        {
            ["Keys"] = keysList
        });
    }
}
