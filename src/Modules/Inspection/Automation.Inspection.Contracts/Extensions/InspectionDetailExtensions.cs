using System.Text.Json;
using Automation.Inspection.Contracts.Dtos;

namespace Automation.Inspection.Contracts.Extensions;

public static class InspectionDetailExtensions
{
    /// <summary>
    /// Lấy giá trị (dạng object/string) từ trường dữ liệu của Inspection theo TagId
    /// </summary>
    public static object? GetValueByTagId(this InspectionDetailDto detail, Guid tagId)
    {
        if (detail.Inspection.Data == null)
            return null;

        // Tìm entry trong TagMap có chứa TagId cần tìm
        var matchedEntry = detail.TagMap.FirstOrDefault(kvp =>
            kvp.Value.Any(t => t.TagId == tagId)
        );

        if (matchedEntry.Value == null || string.IsNullOrWhiteSpace(matchedEntry.Key))
            return null;

        return ExtractJsonValue(detail.Inspection.Data.RootElement, matchedEntry.Key);
    }

    /// <summary>
    /// Lấy danh sách tất cả các giá trị gắn với TagId (trường hợp tag gắn ở nhiều field)
    /// </summary>
    public static IReadOnlyList<object> GetAllValuesByTagId(this InspectionDetailDto detail, Guid tagId)
    {
        if (detail.Inspection.Data == null)
            return Array.Empty<object>();

        var matchedPaths = detail.TagMap
            .Where(kvp => kvp.Value.Any(t => t.TagId == tagId) && !string.IsNullOrWhiteSpace(kvp.Key))
            .Select(kvp => kvp.Key)
            .ToList();

        var values = new List<object>(matchedPaths.Count);
        foreach (var path in matchedPaths)
        {
            var val = ExtractJsonValue(detail.Inspection.Data.RootElement, path);
            if (val is IEnumerable<object> list)
            {
                values.AddRange(list);
            }
            else if (val != null)
            {
                values.Add(val);
            }
        }

        return values;
    }

    /// <summary>
    /// Trích xuất giá trị từ JsonElement theo đường dẫn JSONPath (hỗ trợ dot notation, array indexing: "main_objects[0].name", "main_objects[*].name", "video.resolution", v.v.)
    /// </summary>
    public static object? ExtractJsonValue(this JsonElement root, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return GetElementValue(root);

        // Normalize path: convert "a[0].b" or "a[10][2]" -> "a.0.b" / "a.10.2"
        var normalizedPath = System.Text.RegularExpressions.Regex.Replace(path.TrimStart('$', '.', '/'), @"\[(\d+|\*)\]", ".$1")
            .Replace("[]", ".*");

        var parts = normalizedPath.Split(new[] { '.', '/' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return TraversePath(root, parts, 0);
    }

    private static object? TraversePath(JsonElement current, string[] parts, int index)
    {
        if (index >= parts.Length)
            return GetElementValue(current);

        var segment = parts[index];

        // 1. If segment is a wildcard "*" -> evaluate all array elements
        if (segment == "*")
        {
            if (current.ValueKind == JsonValueKind.Array)
            {
                var list = new List<object>();
                foreach (var item in current.EnumerateArray())
                {
                    var childVal = TraversePath(item, parts, index + 1);
                    if (childVal is IEnumerable<object> subList)
                        list.AddRange(subList);
                    else if (childVal != null)
                        list.Add(childVal);
                }
                return list.Count > 0 ? list : null;
            }
            return null;
        }

        // 2. If segment is an integer index (e.g. "0", "1") and current is Array
        if (int.TryParse(segment, out var arrayIndex))
        {
            if (current.ValueKind == JsonValueKind.Array)
            {
                if (arrayIndex >= 0 && arrayIndex < current.GetArrayLength())
                {
                    return TraversePath(current[arrayIndex], parts, index + 1);
                }
                return null;
            }
        }

        // 3. If current is Object, look up property name
        if (current.ValueKind == JsonValueKind.Object)
        {
            if (current.TryGetProperty(segment, out var next))
            {
                return TraversePath(next, parts, index + 1);
            }

            // Case-insensitive fallback
            foreach (var prop in current.EnumerateObject())
            {
                if (string.Equals(prop.Name, segment, StringComparison.OrdinalIgnoreCase))
                {
                    return TraversePath(prop.Value, parts, index + 1);
                }
            }
            return null;
        }

        return null;
    }

    private static object? GetElementValue(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l)
                ? l
                : element.TryGetDouble(out var d)
                    ? d
                    : element.GetRawText(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => element.EnumerateArray().Select(GetElementValue).Where(x => x != null).ToList()!,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => element.GetRawText()
        };
}
