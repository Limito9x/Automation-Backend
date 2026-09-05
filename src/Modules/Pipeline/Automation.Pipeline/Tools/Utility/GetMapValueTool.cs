using System.Collections;
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

        string ConvertValue(object? v)
        {
            if (v == null) return string.Empty;
            if (v is string s) return s;
            if (v is Guid g) return g.ToString();
            if (v is JsonElement je)
            {
                return je.ValueKind == JsonValueKind.String ? je.GetString() ?? string.Empty : je.GetRawText();
            }
            try
            {
                return JsonSerializer.Serialize(v);
            }
            catch
            {
                return v.ToString() ?? string.Empty;
            }
        }

        if (mapObj is Dictionary<string, string> dStr)
        {
            foreach (var (k, v) in dStr) map[k] = v;
        }
        else if (mapObj is IDictionary<string, object?> dObj)
        {
            foreach (var (k, v) in dObj) map[k] = ConvertValue(v);
        }
        else if (mapObj is IDictionary dNonGeneric)
        {
            foreach (DictionaryEntry de in dNonGeneric)
            {
                if (de.Key != null)
                    map[de.Key.ToString()!] = ConvertValue(de.Value);
            }
        }
        else if (mapObj is JsonElement je && je.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in je.EnumerateObject())
            {
                map[prop.Name] = ConvertValue(prop.Value);
            }
        }
        else if (mapObj is string jsonStr && jsonStr.TrimStart().StartsWith('{'))
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonStr);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        map[prop.Name] = ConvertValue(prop.Value);
                    }
                }
            }
            catch { }
        }

        var found = map.TryGetValue(key, out var val);

        // Fallback linh hoạt cho các key dạng file path (khác biệt giữa '\' vs '/' hoặc full path vs relative path)
        if (!found && !string.IsNullOrWhiteSpace(key))
        {
            var keySlash = key.Replace('\\', '/').Trim('/');
            var keyBackslash = key.Replace('/', '\\').Trim('\\');

            if (map.TryGetValue(keySlash, out val) || map.TryGetValue(keyBackslash, out val))
            {
                found = true;
            }
            else
            {
                var match = map.FirstOrDefault(kv =>
                {
                    var kNorm = kv.Key.Replace('\\', '/').Trim('/');
                    return keySlash.EndsWith("/" + kNorm, StringComparison.OrdinalIgnoreCase) ||
                           kNorm.EndsWith("/" + keySlash, StringComparison.OrdinalIgnoreCase);
                });

                if (!string.IsNullOrEmpty(match.Key))
                {
                    found = true;
                    val = match.Value;
                }
            }
        }

        return Task.FromResult(new Dictionary<string, object>
        {
            ["Value"] = val ?? string.Empty,
            ["Found"] = found
        });
    }
}
