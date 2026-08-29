using System.Collections;
using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Tools.Collections;

public class GetArrayItemTool : IResolverTool
{
    public string Key => "GetArrayItem";
    public string Label => "Get Array Item";
    public string? Category => "Collections";
    public string? Description => "Retrieves the element at a specific index from an Array.";
    public bool IsPure => true;

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new() { Id = "Array", Label = "Array", PrimitiveType = PinPrimitiveType.String, Cardinality = PinCardinality.Array, IsRequired = true },
        new() { Id = "Index", Label = "Index", PrimitiveType = PinPrimitiveType.Number, IsRequired = true },
        new() { Id = "DefaultValue", Label = "Default Value", PrimitiveType = PinPrimitiveType.String, IsRequired = false }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new() { Id = "Item", Label = "Item", PrimitiveType = PinPrimitiveType.String },
        new() { Id = "Found", Label = "Found", PrimitiveType = PinPrimitiveType.Boolean }
    ];

    public Task<Dictionary<string, object>> ExecuteAsync(Dictionary<string, object> inputs, ToolExecutionContext context)
    {
        var targetIndex = 0;
        if (inputs.TryGetValue("Index", out var rawIndex) && rawIndex != null)
        {
            if (rawIndex is int i) targetIndex = i;
            else if (rawIndex is long l) targetIndex = (int)l;
            else if (int.TryParse(rawIndex.ToString(), out var parsed)) targetIndex = parsed;
        }

        var defaultVal = inputs.GetValueOrDefault("DefaultValue");
        var list = new List<object?>();

        if (inputs.TryGetValue("Array", out var rawArray) && rawArray != null)
        {
            if (rawArray is IList iList)
            {
                foreach (var item in iList) list.Add(item);
            }
            else if (rawArray is JsonElement jsonElem && jsonElem.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonElem.EnumerateArray())
                {
                    list.Add(item.ValueKind switch
                    {
                        JsonValueKind.String => item.GetString(),
                        JsonValueKind.Number => item.TryGetInt64(out var num) ? num : item.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => item.GetRawText()
                    });
                }
            }
            else if (rawArray is IEnumerable enumerable && rawArray is not string)
            {
                foreach (var item in enumerable) list.Add(item);
            }
        }

        // Support negative index like -1 for last element
        if (targetIndex < 0 && list.Count > 0)
        {
            targetIndex = list.Count + targetIndex;
        }

        var found = targetIndex >= 0 && targetIndex < list.Count;
        var result = new Dictionary<string, object>
        {
            ["Found"] = found
        };

        if (found && list[targetIndex] != null)
        {
            result["Item"] = list[targetIndex]!;
        }
        else if (defaultVal != null)
        {
            result["Item"] = defaultVal;
        }

        return Task.FromResult(result);
    }
}
