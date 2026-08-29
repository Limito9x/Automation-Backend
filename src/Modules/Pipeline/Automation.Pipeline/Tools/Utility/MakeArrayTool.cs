using System.Collections;
using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Tools.Utility;

public class MakeArrayTool : IResolverTool
{
    public string Key => "MakeArray";
    public string Label => "Make Array";
    public string? Category => "Utility";
    public bool IsPure => true;

    public (IReadOnlyList<PinDefinition> Inputs, IReadOnlyList<PinDefinition> Outputs) ResolvePins(
        Dictionary<string, object?>? configValues,
        IPinResolutionContext? context = null
    )
    {
        if (configValues?.TryGetValue("DynamicPins", out var dpObj) == true && dpObj != null)
        {
            var pinNames = dpObj is IEnumerable<string> strEnum ? strEnum :
                           dpObj is IEnumerable<object> objEnum ? objEnum.Select(x => x.ToString()!) :
                           dpObj is JsonElement jsonEl && jsonEl.ValueKind == JsonValueKind.Array
                               ? jsonEl.EnumerateArray().Select(x => x.GetString()!).Where(x => !string.IsNullOrEmpty(x))
                               : [];

            var dynamicList = pinNames.Select(pinId => new PinDefinition
            {
                Id = pinId,
                Label = pinId.StartsWith("Item_") ? pinId.Replace("_", " ") : pinId,
                PrimitiveType = PinPrimitiveType.String,
                Cardinality = PinCardinality.Single,
                IsRequired = false
            }).ToList();

            if (dynamicList.Count > 0)
            {
                return (dynamicList, Outputs);
            }
        }

        return (Inputs, Outputs);
    }

    public IReadOnlyList<PinDefinition> Inputs =>
        new List<PinDefinition>
        {
            new PinDefinition
            {
                Id = "Items",
                Label = "Items",
                PrimitiveType = PinPrimitiveType.String,
                Cardinality = PinCardinality.Array,
                IsRequired = true,
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
                Cardinality = PinCardinality.Array,
                IsRequired = true,
            }
        };

    public Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    )
    {
        var resultList = new List<string>();

        if (inputs.TryGetValue("Items", out var rawItems) && rawItems != null)
        {
            if (rawItems is IEnumerable<string> strEnumerable)
            {
                resultList.AddRange(strEnumerable.Where(x => !string.IsNullOrEmpty(x)));
            }
            else if (rawItems is string strSingle)
            {
                if (!string.IsNullOrEmpty(strSingle))
                    resultList.Add(strSingle);
            }
            else if (rawItems is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonElement.EnumerateArray())
                {
                    var val = item.GetString() ?? item.GetRawText();
                    if (!string.IsNullOrEmpty(val))
                        resultList.Add(val);
                }
            }
            else if (rawItems is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        var str = item.ToString();
                        if (!string.IsNullOrEmpty(str))
                            resultList.Add(str);
                    }
                }
            }
            else
            {
                var str = rawItems.ToString();
                if (!string.IsNullOrEmpty(str))
                    resultList.Add(str);
            }
        }

        var result = new Dictionary<string, object>
        {
            ["Result"] = resultList.ToArray()
        };

        return Task.FromResult(result);
    }
}
