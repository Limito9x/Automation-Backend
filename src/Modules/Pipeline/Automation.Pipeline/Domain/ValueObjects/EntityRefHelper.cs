using System.Text.Json;

namespace Automation.Pipeline.Domain.ValueObjects;

/// <summary>
/// Helper xử lý chuẩn hóa và giải mã đối tượng EntityReference dạng { "$type": "Resource", "$ref": "guid" }
/// </summary>
public static class EntityRefHelper
{
    public static (string Type, Guid Id, bool IsValid) Parse(object? input)
    {
        if (input == null)
            return (string.Empty, Guid.Empty, false);

        // 1. Direct Guid
        if (input is Guid directGuid && directGuid != Guid.Empty)
        {
            return (string.Empty, directGuid, true);
        }

        // 2. String representation (raw GUID or JSON string)
        var str = input.ToString()?.Trim();
        if (string.IsNullOrEmpty(str))
            return (string.Empty, Guid.Empty, false);

        if (Guid.TryParse(str, out var parsedGuid))
        {
            return (string.Empty, parsedGuid, true);
        }

        // 3. Dictionary / KeyValuePair format
        if (input is IDictionary<string, object?> dict)
        {
            var type = dict.TryGetValue("$type", out var tVal) ? tVal?.ToString() ?? string.Empty : string.Empty;
            var refVal = dict.TryGetValue("$ref", out var rVal) ? rVal?.ToString() ?? string.Empty :
                         dict.TryGetValue("id", out var idVal) ? idVal?.ToString() ?? string.Empty : string.Empty;

            if (Guid.TryParse(refVal, out var dictGuid))
            {
                return (type, dictGuid, true);
            }
        }

        // 4. JsonElement object { "$type": "...", "$ref": "..." }
        if (input is JsonElement element && element.ValueKind == JsonValueKind.Object)
        {
            var type = element.TryGetProperty("$type", out var tProp) ? tProp.GetString() ?? string.Empty : string.Empty;
            var refStr = element.TryGetProperty("$ref", out var rProp) ? rProp.GetString() :
                         element.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;

            if (!string.IsNullOrEmpty(refStr) && Guid.TryParse(refStr, out var elGuid))
            {
                return (type, elGuid, true);
            }
        }

        // 5. JSON serialized string fallback
        if (str.StartsWith('{') && str.EndsWith('}'))
        {
            try
            {
                using var doc = JsonDocument.Parse(str);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    var type = doc.RootElement.TryGetProperty("$type", out var tProp) ? tProp.GetString() ?? string.Empty : string.Empty;
                    var refStr = doc.RootElement.TryGetProperty("$ref", out var rProp) ? rProp.GetString() :
                                 doc.RootElement.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;

                    if (!string.IsNullOrEmpty(refStr) && Guid.TryParse(refStr, out var sGuid))
                    {
                        return (type, sGuid, true);
                    }
                }
            }
            catch
            {
                // Not a valid JSON object
            }
        }

        return (string.Empty, Guid.Empty, false);
    }

    public static Guid? ExtractRefId(object? input)
    {
        var (_, id, isValid) = Parse(input);
        return isValid && id != Guid.Empty ? id : null;
    }

    public static Dictionary<string, object?> Create(string type, Guid id)
    {
        return new Dictionary<string, object?>
        {
            ["$type"] = type,
            ["$ref"] = id.ToString()
        };
    }
}
