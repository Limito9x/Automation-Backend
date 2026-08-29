using System.Collections;
using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;

namespace Automation.Pipeline.Tools.Collections;

public class GetCollectionCountTool : IResolverTool
{
    public string Key => "GetCollectionCount";
    public string Label => "Get Collection Count";
    public string? Category => "Collections";
    public string? Description => "Returns the number of elements in an Array or Map.";
    public bool IsPure => true;

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new() { Id = "Collection", Label = "Collection", PrimitiveType = PinPrimitiveType.String, IsRequired = true }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new() { Id = "Count", Label = "Count", PrimitiveType = PinPrimitiveType.Number }
    ];

    public Task<Dictionary<string, object>> ExecuteAsync(Dictionary<string, object> inputs, ToolExecutionContext context)
    {
        var count = 0;

        if (inputs.TryGetValue("Collection", out var raw) && raw != null)
        {
            if (raw is ICollection col)
            {
                count = col.Count;
            }
            else if (raw is JsonElement jsonElem)
            {
                if (jsonElem.ValueKind == JsonValueKind.Array) count = jsonElem.GetArrayLength();
                else if (jsonElem.ValueKind == JsonValueKind.Object) count = jsonElem.EnumerateObject().Count();
            }
            else if (raw is string str && (str.StartsWith('[') || str.StartsWith('{')))
            {
                try
                {
                    using var doc = JsonDocument.Parse(str);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array) count = doc.RootElement.GetArrayLength();
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object) count = doc.RootElement.EnumerateObject().Count();
                }
                catch { }
            }
            else if (raw is IEnumerable enumerable && raw is not string)
            {
                foreach (var _ in enumerable) count++;
            }
        }

        return Task.FromResult(new Dictionary<string, object>
        {
            ["Count"] = count
        });
    }
}
