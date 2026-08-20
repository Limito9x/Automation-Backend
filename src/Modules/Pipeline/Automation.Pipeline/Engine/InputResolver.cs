using System.Text.Json;
using Automation.Files.Contracts;
using Automation.Pipeline.Domain.Enums;
using Automation.Pipeline.Domain.ValueObjects;
using Automation.Pipeline.Engine.Models;
using Microsoft.Extensions.Logging;

namespace Automation.Pipeline.Engine;

public class InputResolver(ILogger<InputResolver> logger, IAssetApi assetApi) : IInputResolver
{
    public Dictionary<string, object> ResolveInputs(DagNode node, PipelineExecutionState state)
    {
        var resolved = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        for (var pinIdx = 0; pinIdx < node.InputPins.Count; pinIdx++)
        {
            var pin = node.InputPins[pinIdx];
            var pinKey = !string.IsNullOrEmpty(pin.Id) ? pin.Id : pin.Label;
            object? val = null;

            // 1. Check incoming connections (from upstream nodes)
            var connection = node.IncomingConnections.FirstOrDefault(c =>
                string.Equals(c.TargetPinKey, pin.Id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.TargetPinKey, pin.Label, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.TargetPinKey, $"in_{pinIdx}", StringComparison.OrdinalIgnoreCase));

            // If node has only 1 incoming connection and 1 data input pin, match directly
            if (connection == null && node.IncomingConnections.Count == 1 && node.InputPins.Count == 1)
            {
                connection = node.IncomingConnections[0];
            }

            if (connection != null)
            {
                val = state.GetNodeOutput(connection.SourceNodeId, connection.SourcePinKey);
                logger.LogInformation("Node {NodeId} Pin [{PinKey}] matched connection from Node {SourceNodeId} Pin [{SourcePinKey}] => value: {Val}",
                    node.NodeId, pinKey, connection.SourceNodeId, connection.SourcePinKey, val);
            }

            // 2. Check runtime inputs (case-insensitive, alias & fuzzy fallback)
            if (val == null && state.RuntimeInputs.Count > 0)
            {
                var targetKeys = new[]
                {
                    $"{node.NodeId}:{pin.Id}",
                    $"{node.NodeId}:{pin.Label}",
                    pin.Id,
                    pin.Label
                };

                foreach (var tk in targetKeys)
                {
                    if (string.IsNullOrWhiteSpace(tk)) continue;
                    foreach (var (rKey, rVal) in state.RuntimeInputs)
                    {
                        if (string.Equals(rKey, tk, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(rKey.Replace("-", "").Replace("_", ""), tk.Replace("-", "").Replace("_", ""), StringComparison.OrdinalIgnoreCase))
                        {
                            val = rVal;
                            break;
                        }
                    }
                    if (val != null) break;
                }

                // Fuzzy match for common resource / workspace / entity parameters
                if (val == null)
                {
                    foreach (var (rKey, rVal) in state.RuntimeInputs)
                    {
                        if (rVal == null) continue;
                        if ((pin.Id.Contains("resource", StringComparison.OrdinalIgnoreCase) || pin.Label.Contains("resource", StringComparison.OrdinalIgnoreCase)) &&
                            rKey.Contains("resource", StringComparison.OrdinalIgnoreCase))
                        {
                            val = rVal;
                            break;
                        }
                        if ((pin.Id.Contains("workspace", StringComparison.OrdinalIgnoreCase) || pin.Label.Contains("workspace", StringComparison.OrdinalIgnoreCase)) &&
                            rKey.Contains("workspace", StringComparison.OrdinalIgnoreCase))
                        {
                            val = rVal;
                            break;
                        }
                    }
                }
            }

            // 3. Check inline node config
            if (val == null && node.Config != null)
            {
                val = GetValueFromConfig(node.Config, pin.Id) ?? GetValueFromConfig(node.Config, pin.Label);
            }

            // 4. Check pin default value
            if (val == null && pin.DefaultValue != null)
            {
                val = pin.DefaultValue;
            }

            if (val != null)
            {
                var clrVal = NormalizeValue(val);
                if (clrVal != null)
                {
                    // Check if value is an Asset ID (or pin type is Asset)
                    clrVal = ResolveAssetIfApplicable(clrVal, pin);

                    resolved[pinKey] = clrVal;
                    if (!string.IsNullOrEmpty(pin.Id)) resolved[pin.Id] = clrVal;
                    if (!string.IsNullOrEmpty(pin.Label)) resolved[pin.Label] = clrVal;
                }
            }
        }

        return resolved;
    }

    private object ResolveAssetIfApplicable(object clrVal, PinDefinition pin)
    {
        var strVal = clrVal.ToString()?.Trim();
        if (!string.IsNullOrEmpty(strVal) && Guid.TryParse(strVal, out var assetGuid))
        {
            try
            {
                var assetResult = assetApi.GetAssetByIdAsync(assetGuid).GetAwaiter().GetResult();
                if (assetResult.IsSuccess && assetResult.Value != null)
                {
                    var asset = assetResult.Value;
                    logger.LogInformation("Resolved Asset [{AssetId}] -> PublicUrl: {Url} (Filename: {FileName})",
                        assetGuid, asset.PublicUrl, asset.Name);

                    return new Dictionary<string, object?>
                    {
                        {
                            "$file", new Dictionary<string, object?>
                            {
                                { "url", asset.PublicUrl },
                                { "filename", asset.Name },
                                { "hash", asset.Id.ToString("N") },
                                { "size", asset.Size }
                            }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning("Failed to resolve asset ID '{AssetId}': {Error}", assetGuid, ex.Message);
            }
        }

        return clrVal;
    }

    private static object? GetValueFromConfig(JsonDocument config, string key)
    {
        if (string.IsNullOrWhiteSpace(key) || config.RootElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var prop in config.RootElement.EnumerateObject())
        {
            if (string.Equals(prop.Name, key, StringComparison.OrdinalIgnoreCase))
            {
                return prop.Value;
            }
        }

        return null;
    }

    private static object? NormalizeValue(object value)
    {
        if (value is JsonElement element)
        {
            return NormalizeJsonElement(element);
        }

        return value;
    }

    private static object? NormalizeJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => element.EnumerateArray().Select(NormalizeJsonElement).Where(x => x != null).ToArray(),
            JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object?>>(element.GetRawText()),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => element.GetRawText()
        };
    }

}
