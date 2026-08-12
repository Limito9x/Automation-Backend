using System.Text.Json;
using FluentResults;

namespace Automation.DynamicForms.Contracts;

public interface ISchemaApi
{
    // Lấy active schema version để Frontend render Builder/Renderer
    Task<Result<SchemaVersionDto>> GetActiveVersionAsync(string ownerType, string ownerId, CancellationToken ct = default);

    // Validate + Lưu data của một entity
    Task<Result<SchemaDataDto>> SaveDataAsync(string ownerType, string ownerId, string clientId, string clientType, JsonDocument values, CancellationToken ct = default);

    // Lấy data đã lưu theo clientId (để hiện thị lại trên UI Edit)
    Task<Result<SchemaDataDto>> GetDataAsync(string clientId, string clientType, CancellationToken ct = default);

    // Lấy danh sách data đã lưu theo danh sách clientId (để fetch bulk tránh N+1)
    Task<Result<IEnumerable<SchemaDataDto>>> GetMultipleDataAsync(IEnumerable<string> clientIds, string clientType, CancellationToken ct = default);

    // Tạo hoặc cập nhật schema (khi Admin sửa Form Builder)
    Task<Result> UpsertSchemaAsync(string ownerType, string ownerId, string schemaName, JsonDocument fields, CancellationToken ct = default);
}

