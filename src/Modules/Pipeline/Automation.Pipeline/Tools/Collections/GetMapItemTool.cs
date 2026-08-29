using System.Collections;
using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Tools.Collections;

public class GetMapItemTool : IResolverTool
{
    public string Key => "GetMapItem";
    public string Label => "Get Map Item";
    public string? Category => "Collections";
    public string? Description => "Retrieves the value for a specific key in a Map.";
    public bool IsPure => true;

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new() { Id = "Map", Label = "Map", PrimitiveType = PinPrimitiveType.String, Cardinality = PinCardinality.Map, IsRequired = true },
        new() { Id = "Key", Label = "Key", PrimitiveType = PinPrimitiveType.String, IsRequired = true },
        new() { Id = "DefaultValue", Label = "Default Value", PrimitiveType = PinPrimitiveType.String, IsRequired = false }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new() { Id = "Value", Label = "Value", PrimitiveType = PinPrimitiveType.String },
        new() { Id = "Found", Label = "Found", PrimitiveType = PinPrimitiveType.Boolean }
    ];

    public Task<Dictionary<string, object>> ExecuteAsync(Dictionary<string, object> inputs, ToolExecutionContext context)
    {
        var targetKey = inputs.GetValueOrDefault("Key")?.ToString() ?? "";
        var defaultVal = inputs.GetValueOrDefault("DefaultValue");
        object? foundVal = null;
        var found = false;

        if (inputs.TryGetValue("Map", out var rawMap) && rawMap != null)
        {
            if (rawMap is IDictionary dict)
            {
                foreach (DictionaryEntry entry in dict)
                {
                    if (string.Equals(entry.Key?.ToString(), targetKey, StringComparison.OrdinalIgnoreCase))
                    {
                        foundVal = entry.Value;
                        found = true;
                        break;
                    }
                }
            }
            else if (rawMap is JsonElement jsonElem && jsonElem.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in jsonElem.EnumerateObject())
                {
                    if (string.Equals(prop.Name, targetKey, StringComparison.OrdinalIgnoreCase))
                    {
                        foundVal = prop.Value.ValueKind switch
                        {
                            JsonValueKind.String => prop.Value.GetString(),
                            JsonValueKind.Number => prop.Value.TryGetInt64(out var l) ? l : prop.Value.GetDouble(),
                            JsonValueKind.True => true,
                            JsonValueKind.False => false,
                            JsonValueKind.Null => null,
                            _ => prop.Value.GetRawText()
                        };
                        found = true;
                        break;
                    }
                }
            }
            else if (rawMap is string jsonStr && jsonStr.TrimStart().StartsWith('{'))
            {
                try
                {
                    using var doc = JsonDocument.Parse(jsonStr);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        if (string.Equals(prop.Name, targetKey, StringComparison.OrdinalIgnoreCase))
                        {
                            foundVal = prop.Value.GetString() ?? prop.Value.GetRawText();
                            found = true;
                            break;
                        }
                    }
                }
                catch { }
            }
        }

        var result = new Dictionary<string, object>
        {
            ["Found"] = found
        };

        if (found && foundVal != null)
        {
            result["Value"] = foundVal;
        }
        else if (defaultVal != null)
        {
            result["Value"] = defaultVal;
        }

        return Task.FromResult(result);
    }
}
