using System.Text.Json;
using System.Text.Json.Nodes;
using Automation.DynamicForms.Contracts;
using Automation.DynamicForms.Domain.Entities;
using Automation.DynamicForms.Infrastructure.Persistence;
using Automation.SharedKernel.Errors;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.DynamicForms.Infrastructure.Api;

public class SchemaApi(DynamicFormsDbContext db, IServiceProvider serviceProvider) : ISchemaApi
{
    public async Task<Result<SchemaVersionDto>> GetActiveVersionAsync(string ownerType, string ownerId, CancellationToken ct = default)
    {
        var schema = await db.SchemaDefinitions
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.OwnerType == ownerType && s.OwnerId == ownerId, ct);

        if (schema == null)
            return Result.Fail(new NotFoundError($"Schema for {ownerType} {ownerId} not found"));

        var activeVersion = schema.Versions.FirstOrDefault(v => v.IsActive);
        if (activeVersion == null)
            return Result.Fail(new NotFoundError($"Active schema version for {ownerType} {ownerId} not found"));

        return new SchemaVersionDto
        {
            Id = activeVersion.Id,
            SchemaDefinitionId = activeVersion.SchemaDefinitionId,
            Fields = activeVersion.Fields,
            Version = activeVersion.Version,
            IsActive = activeVersion.IsActive
        };
    }

    public async Task<Result<SchemaDataDto>> SaveDataAsync(string ownerType, string ownerId, string clientId, string clientType, JsonDocument values, CancellationToken ct = default)
    {
        // 1. Check if ownerType is registered
        var registeredSchemas = serviceProvider.GetServices<RegisteredDynamicSchema>();
        if (!registeredSchemas.Any(r => r.OwnerType == ownerType))
            return Result.Fail(new Error($"OwnerType '{ownerType}' is not registered to use DynamicForms."));

        // 2. Get active schema version
        var schema = await db.SchemaDefinitions
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.OwnerType == ownerType && s.OwnerId == ownerId, ct);

        if (schema == null)
            return Result.Fail(new NotFoundError($"Schema for {ownerType} {ownerId} not found"));

        var activeVersion = schema.Versions.FirstOrDefault(v => v.IsActive);
        if (activeVersion == null)
            return Result.Fail(new NotFoundError($"Active schema version for {ownerType} {ownerId} not found"));

        // 3. Validation Engine (MVP)
        var validationResult = ValidateValues(activeVersion.Fields, values);
        if (validationResult.IsFailed)
            return validationResult;

        // 4. Auto-migration / Data normalization
        var normalizedValues = NormalizeValues(activeVersion.Fields, values);

        // 5. Save or Update
        var existingData = await db.SchemaData
            .FirstOrDefaultAsync(d => d.ClientId == clientId && d.ClientType == clientType, ct);

        if (existingData != null)
        {
            existingData.UpdateValues(normalizedValues, activeVersion.Id);
        }
        else
        {
            existingData = new SchemaData(activeVersion.Id, normalizedValues, clientId, clientType);
            db.SchemaData.Add(existingData);
        }

        await db.SaveChangesAsync(ct);

        return new SchemaDataDto
        {
            Id = existingData.Id,
            SchemaVersionId = existingData.SchemaVersionId,
            Values = existingData.Values,
            ClientId = existingData.ClientId,
            ClientType = existingData.ClientType
        };
    }

    public async Task<Result<SchemaDataDto>> GetDataAsync(string clientId, string clientType, CancellationToken ct = default)
    {
        var data = await db.SchemaData
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ClientId == clientId && d.ClientType == clientType, ct);

        if (data == null)
            return Result.Fail(new NotFoundError($"Schema data for {clientType} {clientId} not found"));

        return new SchemaDataDto
        {
            Id = data.Id,
            SchemaVersionId = data.SchemaVersionId,
            Values = data.Values,
            ClientId = data.ClientId,
            ClientType = data.ClientType
        };
    }

    public async Task<Result> UpsertSchemaAsync(string ownerType, string ownerId, string schemaName, JsonDocument fields, CancellationToken ct = default)
    {
        // Check if ownerType is registered
        var registeredSchemas = serviceProvider.GetServices<RegisteredDynamicSchema>();
        if (!registeredSchemas.Any(r => r.OwnerType == ownerType))
            return Result.Fail(new Error($"OwnerType '{ownerType}' is not registered to use DynamicForms."));

        var schema = await db.SchemaDefinitions
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.OwnerType == ownerType && s.OwnerId == ownerId, ct);

        if (schema == null)
        {
            schema = new SchemaDefinition(schemaName, ownerId, ownerType);
            db.SchemaDefinitions.Add(schema);
            
            var initialVersion = new SchemaVersion(schema.Id, fields, 1, true);
            db.SchemaVersions.Add(initialVersion);
        }
        else
        {
            // Update name if changed
            var activeVersion = schema.Versions.FirstOrDefault(v => v.IsActive);
            
            // Check if fields actually changed to avoid creating unnecessary versions
            if (activeVersion != null && JsonSerializer.Serialize(activeVersion.Fields) == JsonSerializer.Serialize(fields))
            {
                return Result.Ok();
            }

            if (activeVersion != null)
            {
                activeVersion.Deactivate();
            }

            var nextVersionNumber = schema.Versions.Any() ? schema.Versions.Max(v => v.Version) + 1 : 1;
            var newVersion = new SchemaVersion(schema.Id, fields, nextVersionNumber, true);
            db.SchemaVersions.Add(newVersion);
        }

        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private Result ValidateValues(JsonDocument schemaFields, JsonDocument values)
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

            // TODO: Bổ sung các rule phức tạp hơn (min, max, pattern, custom type validation) ở đây trong tương lai.
            // MVP version chỉ bắt required.
        }

        return errors.Any() ? Result.Fail(errors) : Result.Ok();
    }

    private JsonDocument NormalizeValues(JsonDocument schemaFields, JsonDocument values)
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
}
