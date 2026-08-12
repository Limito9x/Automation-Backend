using System.Text.Json;
using Automation.DynamicForms.Contracts;
using Automation.DynamicForms.Domain.Entities;
using Automation.DynamicForms.Infrastructure.Persistence;
using Automation.DynamicForms.Services;
using Microsoft.EntityFrameworkCore;

namespace Automation.DynamicForms.Infrastructure.Api;

public class SchemaApi(DynamicFormsDbContext db, IEnumerable<RegisteredDynamicSchema> registeredSchemas, IDynamicFormEngine engine) : ISchemaApi
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
        var validationResult = engine.ValidateValues(activeVersion.Fields, values);
        if (validationResult.IsFailed)
            return validationResult;

        // 4. Auto-migration / Data normalization
        var normalizedValues = engine.NormalizeValues(activeVersion.Fields, values);

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

        // 6. Link file fields (if any) to asset links
        await engine.LinkFileFieldsAsync(existingData.Id.ToString(), activeVersion.Fields, normalizedValues, ct);

        // 7. Resolve data for response
        var resolvedDataResult = await engine.ResolveDataAsync(existingData.Id.ToString(), activeVersion.Fields, existingData.Values, ct);

        return new SchemaDataDto
        {
            Id = existingData.Id,
            SchemaVersion = activeVersion.Fields,
            Values = existingData.Values,
            ResolvedData = resolvedDataResult.IsSuccess ? resolvedDataResult.Value : existingData.Values,
            ClientId = existingData.ClientId,
            ClientType = existingData.ClientType
        };
    }

    public async Task<Result<SchemaDataDto>> GetDataAsync(string clientId, string clientType, CancellationToken ct = default)
    {
        var data = await db.SchemaData
            .Include(d => d.SchemaVersion)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.ClientId == clientId && d.ClientType == clientType, ct);

        if (data == null)
            return Result.Fail(new NotFoundError($"Schema data for {clientType} {clientId} not found"));

        var resolvedDataResult = await engine.ResolveDataAsync(data.Id.ToString(), data.SchemaVersion.Fields, data.Values, ct);

        return new SchemaDataDto
        {
            Id = data.Id,
            SchemaVersion = data.SchemaVersion.Fields,
            Values = data.Values,
            ResolvedData = resolvedDataResult.IsSuccess ? resolvedDataResult.Value : data.Values,
            ClientId = data.ClientId,
            ClientType = data.ClientType
        };
    }

    public async Task<Result<IEnumerable<SchemaDataDto>>> GetMultipleDataAsync(IEnumerable<string> clientIds, string clientType, CancellationToken ct = default)
    {
        var clientIdsList = clientIds.ToList();
        
        var data = await db.SchemaData
            .Include(d => d.SchemaVersion)
            .AsNoTracking()
            .Where(d => clientIdsList.Contains(d.ClientId) && d.ClientType == clientType)
            .ToListAsync(ct);

        var dtos = new List<SchemaDataDto>();
        foreach (var d in data)
        {
            var resolvedDataResult = await engine.ResolveDataAsync(d.Id.ToString(), d.SchemaVersion.Fields, d.Values, ct);
            dtos.Add(new SchemaDataDto
            {
                Id = d.Id,
                SchemaVersion = d.SchemaVersion.Fields,
                Values = d.Values,
                ResolvedData = resolvedDataResult.IsSuccess ? resolvedDataResult.Value : d.Values,
                ClientId = d.ClientId,
                ClientType = d.ClientType
            });
        }

        return Result.Ok<IEnumerable<SchemaDataDto>>(dtos);
    }

    public async Task<Result> UpsertSchemaAsync(string ownerType, string ownerId, string schemaName, JsonDocument fields, CancellationToken ct = default)
    {
        // Check if ownerType is registered
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
}



