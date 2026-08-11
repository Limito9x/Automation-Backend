using System.Text.Json;
using System.Text.Json.Nodes;
using Automation.DynamicForms.Constants;
using Automation.DynamicForms.Domain.Entities;
using Automation.Files.Contracts;
using FluentResults;

namespace Automation.DynamicForms.Services;

public class DynamicFormEngine(IAssetApi assetApi) : IDynamicFormEngine
{
    public Result ValidateValues(JsonDocument schemaFields, JsonDocument values)
    {
        var fieldsArray = schemaFields.RootElement.ValueKind == JsonValueKind.Array 
            ? schemaFields.RootElement.EnumerateArray().ToList() 
            : new List<JsonElement>();

        var valueObj = values.RootElement.ValueKind == JsonValueKind.Object 
            ? values.RootElement 
            : default;

        var errors = new List<IError>();

        foreach (var fieldDef in fieldsArray)
        {
            var fieldName = fieldDef.GetProperty("name").GetString();
            if (string.IsNullOrEmpty(fieldName)) continue;

            var properties = fieldDef.TryGetProperty("properties", out var props) ? props : default;
            var isRequired = properties.ValueKind == JsonValueKind.Object && properties.TryGetProperty("required", out var req) && req.GetBoolean();

            JsonElement fieldValue = default;
            bool hasValue = valueObj.ValueKind == JsonValueKind.Object && valueObj.TryGetProperty(fieldName, out fieldValue);
            bool isNullOrEmpty = !hasValue || 
                                 fieldValue.ValueKind == JsonValueKind.Null || 
                                 (fieldValue.ValueKind == JsonValueKind.String && string.IsNullOrEmpty(fieldValue.GetString()));

            if (isRequired && isNullOrEmpty)
            {
                var reqMsg = properties.TryGetProperty("requiredMsg", out var msg) ? msg.GetString() : $"{fieldName} is required";
                errors.Add(new Error(reqMsg).WithMetadata("Field", fieldName));
            }

            // TODO: Add more complex validation rules (min, max, pattern, custom type validation) here in the future.
            // MVP version only catches required.
        }

        return errors.Any() ? Result.Fail(errors) : Result.Ok();
    }

    public JsonDocument NormalizeValues(JsonDocument schemaFields, JsonDocument values)
    {
        // Tự động migrate: thêm các field mới bằng null, loại bỏ các field thừa khỏi values
        var fieldsArray = schemaFields.RootElement.ValueKind == JsonValueKind.Array 
            ? schemaFields.RootElement.EnumerateArray().ToList() 
            : new List<JsonElement>();

        var valueObj = values.RootElement.ValueKind == JsonValueKind.Object 
            ? values.RootElement 
            : default;

        var normalizedNode = new JsonObject();

        foreach (var fieldDef in fieldsArray)
        {
            var fieldName = fieldDef.GetProperty("name").GetString();
            if (string.IsNullOrEmpty(fieldName)) continue;

            if (valueObj.ValueKind == JsonValueKind.Object && valueObj.TryGetProperty(fieldName, out var fieldValue))
            {
                normalizedNode[fieldName] = JsonNode.Parse(fieldValue.GetRawText());
            }
            else
            {
                normalizedNode[fieldName] = null;
            }
        }

        return JsonDocument.Parse(normalizedNode.ToJsonString());
    }

    public async Task<Result> LinkFileFieldsAsync(string schemaDataId, JsonDocument schemaFields, JsonDocument values, CancellationToken ct = default)
    {
        var fieldsArray = schemaFields.RootElement.ValueKind == JsonValueKind.Array 
            ? schemaFields.RootElement.EnumerateArray().ToList() 
            : new List<JsonElement>();

        var valueObj = values.RootElement.ValueKind == JsonValueKind.Object 
            ? values.RootElement 
            : default;

        if (valueObj.ValueKind != JsonValueKind.Object)
            return Result.Ok();

        var assetUpsertDtos = new List<AssetUpsertDto>();

        foreach (var fieldDef in fieldsArray)
        {
            var fieldName = fieldDef.GetProperty("name").GetString();
            if (string.IsNullOrEmpty(fieldName)) continue;

            var fieldType = fieldDef.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;
            if (fieldType != SchemaType.File) continue;

            if (valueObj.TryGetProperty(fieldName, out var fieldValue) && fieldValue.ValueKind != JsonValueKind.Null)
            {
                ExtractAssetUpserts(fieldValue, assetUpsertDtos);
            }
        }

        if (assetUpsertDtos.Count > 0)
        {
            return await assetApi.UpsertMultipleAsync(
                nameof(SchemaData),
                schemaDataId,
                DynamicFormAssets.SchemaDataAsset,
                assetUpsertDtos,
                ct);
        }

        return Result.Ok();
    }

    public async Task<Result<JsonDocument>> ResolveDataAsync(string schemaDataId, JsonDocument schemaFields, JsonDocument values, CancellationToken ct = default)
    {
        var fieldsArray = schemaFields.RootElement.ValueKind == JsonValueKind.Array 
            ? schemaFields.RootElement.EnumerateArray().ToList() 
            : new List<JsonElement>();

        var valueObj = values.RootElement.ValueKind == JsonValueKind.Object 
            ? values.RootElement 
            : default;

        var resolvedNode = new JsonObject();

        var referencedAssetIds = new List<string>();

        foreach (var fieldDef in fieldsArray)
        {
            var fieldName = fieldDef.GetProperty("name").GetString();
            if (string.IsNullOrEmpty(fieldName)) continue;

            var fieldType = fieldDef.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;
            if (fieldType != SchemaType.File) continue;

            if (valueObj.ValueKind == JsonValueKind.Object && valueObj.TryGetProperty(fieldName, out var fieldValue) && fieldValue.ValueKind != JsonValueKind.Null)
            {
                ExtractAssetIdStrings(fieldValue, referencedAssetIds);
            }
        }

        var assetMap = new Dictionary<string, AssetDto>(StringComparer.OrdinalIgnoreCase);
        if (referencedAssetIds.Count > 0)
        {
            var assetsResult = await assetApi.GetAssetsByIdsAsync(
                nameof(SchemaData),
                schemaDataId,
                DynamicFormAssets.SchemaDataAsset,
                referencedAssetIds.Distinct(),
                ct);

            if (assetsResult.IsSuccess && assetsResult.Value != null)
            {
                foreach (var asset in assetsResult.Value)
                {
                    assetMap[asset.Id.ToString()] = asset;
                }
            }
        }

        var camelCaseOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        foreach (var fieldDef in fieldsArray)
        {
            var fieldName = fieldDef.GetProperty("name").GetString();
            if (string.IsNullOrEmpty(fieldName)) continue;

            var fieldType = fieldDef.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;

            if (valueObj.ValueKind == JsonValueKind.Object && valueObj.TryGetProperty(fieldName, out var fieldValue) && fieldValue.ValueKind != JsonValueKind.Null)
            {
                if (fieldType == SchemaType.File)
                {
                    var resolvedAssetList = ResolveAssetDtosForField(fieldValue, assetMap);
                    resolvedNode[fieldName] = JsonNode.Parse(JsonSerializer.Serialize(resolvedAssetList, camelCaseOptions));
                }
            }
        }

        return JsonDocument.Parse(resolvedNode.ToJsonString());
    }

    private static void ExtractAssetUpserts(JsonElement element, List<AssetUpsertDto> dtos)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                ExtractSingleAssetUpsert(item, dtos);
            }
        }
        else
        {
            ExtractSingleAssetUpsert(element, dtos);
        }
    }

    private static void ExtractSingleAssetUpsert(JsonElement item, List<AssetUpsertDto> dtos)
    {
        if (item.ValueKind == JsonValueKind.Object)
        {
            Guid assetId = Guid.Empty;
            if (item.TryGetProperty("assetId", out var idProp) && idProp.ValueKind == JsonValueKind.String)
            {
                Guid.TryParse(idProp.GetString(), out assetId);
            }
            else if (item.TryGetProperty("id", out var idProp2) && idProp2.ValueKind == JsonValueKind.String)
            {
                Guid.TryParse(idProp2.GetString(), out assetId);
            }

            if (assetId != Guid.Empty)
            {
                string name = string.Empty;
                if (item.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                {
                    name = nameProp.GetString() ?? string.Empty;
                }
                else if (item.TryGetProperty("originalName", out var nameProp2) && nameProp2.ValueKind == JsonValueKind.String)
                {
                    name = nameProp2.GetString() ?? string.Empty;
                }

                dtos.Add(new AssetUpsertDto { AssetId = assetId, Name = name });
            }
        }
        else if (item.ValueKind == JsonValueKind.String)
        {
            if (Guid.TryParse(item.GetString(), out var assetId))
            {
                dtos.Add(new AssetUpsertDto { AssetId = assetId, Name = string.Empty });
            }
        }
    }

    private static void ExtractAssetIdStrings(JsonElement element, List<string> ids)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                ExtractSingleAssetIdString(item, ids);
            }
        }
        else
        {
            ExtractSingleAssetIdString(element, ids);
        }
    }

    private static void ExtractSingleAssetIdString(JsonElement item, List<string> ids)
    {
        if (item.ValueKind == JsonValueKind.Object)
        {
            if (item.TryGetProperty("assetId", out var idProp) && idProp.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(idProp.GetString()))
            {
                ids.Add(idProp.GetString()!);
            }
            else if (item.TryGetProperty("id", out var idProp2) && idProp2.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(idProp2.GetString()))
            {
                ids.Add(idProp2.GetString()!);
            }
        }
        else if (item.ValueKind == JsonValueKind.String && !string.IsNullOrEmpty(item.GetString()))
        {
            ids.Add(item.GetString()!);
        }
    }

    private static List<AssetDto> ResolveAssetDtosForField(JsonElement element, Dictionary<string, AssetDto> assetMap)
    {
        var result = new List<AssetDto>();

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var dto = ResolveSingleAssetDto(item, assetMap);
                if (dto != null) result.Add(dto);
            }
        }
        else
        {
            var dto = ResolveSingleAssetDto(element, assetMap);
            if (dto != null) result.Add(dto);
        }

        return result;
    }

    private static AssetDto? ResolveSingleAssetDto(JsonElement item, Dictionary<string, AssetDto> assetMap)
    {
        string? assetIdStr = null;
        string? customName = null;

        if (item.ValueKind == JsonValueKind.Object)
        {
            if (item.TryGetProperty("assetId", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                assetIdStr = idProp.GetString();
            else if (item.TryGetProperty("id", out var idProp2) && idProp2.ValueKind == JsonValueKind.String)
                assetIdStr = idProp2.GetString();

            if (item.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                customName = nameProp.GetString();
            else if (item.TryGetProperty("originalName", out var nameProp2) && nameProp2.ValueKind == JsonValueKind.String)
                customName = nameProp2.GetString();
        }
        else if (item.ValueKind == JsonValueKind.String)
        {
            assetIdStr = item.GetString();
        }

        if (assetIdStr != null && assetMap.TryGetValue(assetIdStr, out var assetDto))
        {
            if (!string.IsNullOrEmpty(customName))
            {
                return assetDto with { Name = customName };
            }
            return assetDto;
        }

        return null;
    }
}

