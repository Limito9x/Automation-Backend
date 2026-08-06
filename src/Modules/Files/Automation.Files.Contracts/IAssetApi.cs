using FluentResults;

namespace Automation.Files.Contracts;

public interface IAssetApi
{
    // Request Upload Multiple
    Task<Result<IEnumerable<AssetUploadDto>>> RequestUploadAsync(IEnumerable<UploadRequestItemDto> requests, CancellationToken ct = default);

    // Overload cho 1 file
    async Task<Result<AssetUploadDto>> RequestUploadAsync(string hashSha256, string extension, long sizeBytes, string contentType, CancellationToken ct = default)
    {
        var result = await RequestUploadAsync(new[] { new UploadRequestItemDto(hashSha256, extension, sizeBytes, contentType) }, ct);
        return result.IsSuccess ? Result.Ok(result.Value.First()) : result.ToResult();
    }

    // Confirm Upload Multiple
    Task<Result> ConfirmUploadAsync(IEnumerable<Guid> assetIds, CancellationToken ct = default);

    // Overload cho 1 file
    async Task<Result> ConfirmUploadAsync(Guid assetId, CancellationToken ct = default)
        => await ConfirmUploadAsync(new[] { assetId }, ct);

    // Verify And Link Multiple
    Task<Result> VerifyAndLinkAsync(IEnumerable<AssetLinkRequestItem> items, string ownerEntityType, string slotKey, string ownerEntityId, int startSortOrder = 0, CancellationToken ct = default);

    // Overload cho 1 file
    async Task<Result> VerifyAndLinkAsync(Guid assetId, string ownerEntityType, string slotKey, string ownerEntityId, string originalName, int sortOrder = 0, CancellationToken ct = default)
        => await VerifyAndLinkAsync(new[] { new AssetLinkRequestItem(assetId, originalName) }, ownerEntityType, slotKey, ownerEntityId, sortOrder, ct);

    // Xóa link
    Task<Result> RemoveLinkAsync(Guid assetId, string ownerEntityId, CancellationToken ct = default);

    // Query file theo slot
    Task<Result<IReadOnlyList<AssetLinkDto>>> GetFilesAsync(string ownerEntityId, string ownerEntityType, string slotKey, CancellationToken ct = default);
    
    // Query toàn bộ file của một Entity
    Task<Result<ILookup<string, AssetLinkDto>>> GetAllFilesForEntityAsync(string ownerEntityId, string ownerEntityType, CancellationToken ct = default);
}

