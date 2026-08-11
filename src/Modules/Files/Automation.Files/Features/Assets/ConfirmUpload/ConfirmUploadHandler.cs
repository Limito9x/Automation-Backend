using Automation.Files.Contracts;
using FluentResults;

namespace Automation.Files.Features.Assets.ConfirmUpload;

public class ConfirmUploadHandler(IAssetApi assetApi)
{
    public async Task<Result<IReadOnlyList<ConfirmAssetDto>>> HandleAsync(ConfirmUploadCommand command, CancellationToken ct)
    {
        return await assetApi.ConfirmUploadAsync(command.AssetIds, ct);
    }
}


