using FluentResults;

namespace Automation.Platform.Contracts;

public interface IPlatformApi
{
    Task<Result<IReadOnlyList<string>>> GetAllowedExtensionsAsync(Guid platformId, CancellationToken ct = default);
    Task<Result<Guid?>> GetExtensionIdAsync(Guid platformId, string extension, CancellationToken ct = default);
}
