using System.Text.Json;
using FluentResults;

namespace Automation.DynamicForms.Services;

public interface IDynamicFormEngine
{
    Result ValidateValues(JsonDocument schemaFields, JsonDocument values);
    JsonDocument NormalizeValues(JsonDocument schemaFields, JsonDocument values);
    Task<Result> LinkFileFieldsAsync(string schemaDataId, JsonDocument schemaFields, JsonDocument values, CancellationToken ct = default);
    Task<Result<JsonDocument>> ResolveDataAsync(string schemaDataId, JsonDocument schemaFields, JsonDocument values, CancellationToken ct = default);
}

