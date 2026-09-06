using System.Text.Json;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Engine.DataResolver;

namespace Automation.Pipeline.Tools.Utility;

/// <summary>
/// Tool gán hoặc thêm một cặp [Key, Value] vào Map (Set Map Key / Map Add) tương tự Unreal Engine.
/// Hỗ trợ cả 2 chế độ:
/// 1. Variable Mode: Nhập tên biến (VariableName) để cập nhật thẳng vào biến Execution Context.
/// 2. Wire Mode: Cắm dây vào TargetMap để nhận và xuất Map qua data wire.
/// </summary>
public class SetMapKeyTool(IExecutionMemoryStore? memoryStore = null) : IResolverTool
{
    public string Key => "SetMapKey";
    public string Label => "Set Map Key";
    public string? Category => "Utility";
    public bool IsPure => false;

    public IReadOnlyList<PinDefinition> Inputs =>
    [
        new()
        {
            Id = "VariableName",
            Label = "Variable Name",
            PrimitiveType = PinPrimitiveType.EntityRef,
            EntityTarget = "variable",
            Cardinality = PinCardinality.Single,
            IsRequired = false
        },
        new()
        {
            Id = "TargetMap",
            Label = "Target Map",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Map,
            IsRequired = false
        },
        new()
        {
            Id = "Key",
            Label = "Key",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single,
            IsRequired = true
        },
        new()
        {
            Id = "Value",
            Label = "Value",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Single,
            IsRequired = false,
            DefaultValue = ""
        }
    ];

    public IReadOnlyList<PinDefinition> Outputs =>
    [
        new()
        {
            Id = "Result",
            Label = "Result Map",
            PrimitiveType = PinPrimitiveType.String,
            Cardinality = PinCardinality.Map,
            IsRequired = true
        }
    ];

    public async Task<Dictionary<string, object>> ExecuteAsync(
        Dictionary<string, object> inputs,
        ToolExecutionContext context
    )
    {
        var map = new Dictionary<string, string>();
        var varName = inputs.GetValueOrDefault("VariableName")?.ToString()
                      ?? inputs.GetValueOrDefault("variablename")?.ToString();

        // 1. Nếu có VariableName, đọc Map hiện tại từ Variable Store
        if (!string.IsNullOrWhiteSpace(varName) && context.PipelineExecutionId != Guid.Empty && memoryStore != null)
        {
            var existingVar = await memoryStore.GetVariableAsync(context.PipelineExecutionId, varName, context.CancellationToken);
            MergeToMap(existingVar, map);
        }

        // 2. Sao chép các phần tử từ giá trị tích lũy trước đó của chính node này trong lượt chạy hiện tại
        if (context.PipelineExecutionId != Guid.Empty && context.NodeId != Guid.Empty && memoryStore != null)
        {
            var prevAccumulated = await memoryStore.GetNodePinValueAsync(context.PipelineExecutionId, context.NodeId, "Result", scope: null, context.CancellationToken);
            MergeToMap(prevAccumulated, map);
        }

        // 3. Sao chép các phần tử từ TargetMap nếu có qua dây wire
        var targetMapObj = inputs.GetValueOrDefault("TargetMap") ?? inputs.GetValueOrDefault("targetmap");
        MergeToMap(targetMapObj, map);

        // 4. Gán/thêm Key - Value
        var key = inputs.GetValueOrDefault("Key")?.ToString();
        var val = inputs.GetValueOrDefault("Value")?.ToString() ?? string.Empty;

        if (!string.IsNullOrEmpty(key))
        {
            map[key] = val;
        }

        // 5. Nếu có VariableName, cập nhật lại vào Variable Store
        if (!string.IsNullOrWhiteSpace(varName) && context.PipelineExecutionId != Guid.Empty && memoryStore != null)
        {
            await memoryStore.SetVariableAsync(context.PipelineExecutionId, varName, map, context.CancellationToken);
        }

        // 6. Lưu giá trị tích lũy vào scope global (null) để node bên ngoài vòng lặp đọc được trực tiếp
        if (context.PipelineExecutionId != Guid.Empty && context.NodeId != Guid.Empty && memoryStore != null)
        {
            await memoryStore.SetNodePinValueAsync(context.PipelineExecutionId, context.NodeId, "Result", map, scope: null, context.CancellationToken);
            await memoryStore.SetNodePinValueAsync(context.PipelineExecutionId, context.NodeId, "Result Map", map, scope: null, context.CancellationToken);
        }

        return new Dictionary<string, object>
        {
            ["Result"] = map,
            ["Result Map"] = map
        };
    }

    private static void MergeToMap(object? source, Dictionary<string, string> target)
    {
        if (source == null) return;

        if (source is IDictionary<string, string> dStr)
        {
            foreach (var (k, v) in dStr) target[k] = v;
        }
        else if (source is IDictionary<string, object?> dObj)
        {
            foreach (var (k, v) in dObj) target[k] = v?.ToString() ?? string.Empty;
        }
        else if (source is JsonElement jsonElem && jsonElem.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in jsonElem.EnumerateObject())
            {
                target[prop.Name] = prop.Value.GetString() ?? prop.Value.GetRawText();
            }
        }
        else if (source is string jsonStr && jsonStr.TrimStart().StartsWith('{'))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonStr);
                if (parsed != null)
                {
                    foreach (var (k, v) in parsed) target[k] = v;
                }
            }
            catch { }
        }
    }
}
